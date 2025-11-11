# 🔍 PayOS Webhook Debug Guide
**Date:** November 11, 2025  
**Issue:** Payments stuck in PENDING status after successful payment  
**Domain:** https://trippio.azurewebsites.net

---

## 🎯 ROOT CAUSE IDENTIFIED

### ❌ **Bug: OrderCode Mismatch**

**Before Fix:**
```csharp
// CheckoutController.cs - Line 145-155
long orderCode = (timestamp % 1000000) * 1000 + random;  // Generate orderCode
var createResult = await _payOS.createPaymentLink(paymentData);

// ❌ WRONG: Save with generated orderCode
await _payments.CreateAsync(new CreatePaymentRequest {
    OrderCode = orderCode  // ← This might NOT match PayOS's orderCode!
});
```

**After Fix:**
```csharp
var createResult = await _payOS.createPaymentLink(paymentData);
var payOSOrderCode = createResult.orderCode;  // ← Use PayOS's orderCode

// ✅ CORRECT: Save with PayOS's orderCode
await _payments.CreateAsync(new CreatePaymentRequest {
    OrderCode = payOSOrderCode  // ← This WILL match webhook OrderCode!
});
```

**Why this matters:**
- PayOS may modify/validate the orderCode you send
- Webhook will use **PayOS's OrderCode**, not yours
- If OrderCode doesn't match → Payment not found → Status stays PENDING

---

## 📋 CHECKLIST: Ensure Webhook Works

### 1. ✅ **Verify Webhook URL on PayOS Dashboard**

**Login:** https://my.payos.vn  
**Navigate:** Settings → Webhooks  
**Expected URL:** `https://trippio.azurewebsites.net/api/payment/payos-callback`

**Test Command:**
```bash
curl -X POST https://trippio.azurewebsites.net/api/payment/payos-callback \
  -H "Content-Type: application/json" \
  -d '{"code":"00","desc":"Success","data":{"orderCode":123456,"amount":10000,"description":"Test","accountNumber":"","reference":"","transactionDateTime":"2024-11-11 12:00:00","currency":"VND","paymentLinkId":"test123","code":"00","desc":"Success","counterAccountBankId":"","counterAccountBankName":"","counterAccountName":"","counterAccountNumber":"","virtualAccountName":"","virtualAccountNumber":""},"signature":"test"}'
```

**Expected Response:**
```json
{
  "success": false,
  "message": "Invalid webhook signature"
}
```
(Signature will be invalid, but endpoint should respond!)

---

### 2. ✅ **Check Azure Logs for Webhook Calls**

**Via Azure Portal:**
1. Go to: https://portal.azure.com
2. Navigate to: trippio.azurewebsites.net → Monitoring → Log Stream
3. Look for logs with emojis: 🔔 🔐 ✅ ❌ 💰

**Via Kudu:**
1. Go to: https://trippio.scm.azurewebsites.net/DebugConsole
2. Navigate to: `LogFiles/Application/`
3. Check latest log file for webhook entries

**Expected Log Pattern:**
```
🔔 PayOS Webhook Received! Raw Data: {webhook json}
🔐 Signature Verification - OrderCode: 123456, SignatureData: '123456|10000|00|REF123'
✅ Webhook signature verified for OrderCode: 123456
💰 Payment SUCCESSFUL for OrderCode: 123456, Amount: 10000
🔄 Starting payment status update for OrderCode: 123456...
✅ SUCCESS: Payment status updated to PAID for OrderCode: 123456
✅ Webhook processed successfully for OrderCode: 123456
```

**If NO logs appear:**
- Webhook URL not configured correctly on PayOS dashboard
- Firewall blocking PayOS IP addresses
- Azure network security group blocking incoming requests

---

### 3. ✅ **Test Payment Flow End-to-End**

#### Step 1: Create Payment
```bash
# Login and get JWT token first
curl -X POST https://trippio.azurewebsites.net/api/Checkout/start \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "buyerName": "Test User",
    "buyerEmail": "test@example.com",
    "buyerPhone": "0123456789",
    "platform": "web"
  }'
```

**Expected Response:**
```json
{
  "code": 200,
  "data": {
    "checkoutUrl": "https://pay.payos.vn/...",
    "orderCode": 771673,  // ← Note this OrderCode
    "amount": 10000,
    "status": "PENDING"
  }
}
```

#### Step 2: Check Database
```sql
-- Connect to database
SELECT 
    Id, 
    OrderCode, 
    Amount, 
    Status, 
    PaymentMethod,
    DateCreated,
    ModifiedDate
FROM Payments
WHERE OrderCode = 771673;  -- Use OrderCode from Step 1
```

**Expected:**
```
OrderCode: 771673
Status: Pending
PaymentMethod: PayOS
```

#### Step 3: Complete Payment
1. Open `checkoutUrl` in browser
2. Complete test payment using PayOS sandbox
3. Wait 5-10 seconds for webhook

#### Step 4: Verify Status Updated
```sql
SELECT * FROM Payments WHERE OrderCode = 771673;
```

**Expected:**
```
Status: Paid  ← Should be updated from Pending to Paid
ModifiedDate: 2025-11-11 08:00:00  ← Should be updated
```

---

### 4. ✅ **Manual Webhook Test (If Auto-Webhook Fails)**

**Endpoint:** `POST /api/payment/test-webhook/{orderCode}`

```bash
curl -X POST https://trippio.azurewebsites.net/api/payment/test-webhook/771673 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Payment for OrderCode 771673 marked as PAID"
}
```

---

## 🔧 TROUBLESHOOTING

### Issue 1: Webhook NOT Called
**Symptoms:** No webhook logs in Azure, payment stays PENDING

**Solution:**
1. Verify webhook URL on PayOS dashboard
2. Check Azure firewall/NSG rules
3. Test webhook endpoint manually (Step 1 in Checklist)
4. Contact PayOS support to verify webhook configuration

### Issue 2: Webhook Called but Signature Invalid
**Symptoms:** Logs show `❌ Invalid webhook signature`

**Solution:**
1. Verify ChecksumKey in `appsettings.json` matches PayOS dashboard
2. Check signature computation algorithm:
   ```csharp
   var signatureData = $"{orderCode}|{amount}|{code}|{reference}";
   var signature = HMACSHA256(signatureData, checksumKey);
   ```
3. Log signature components for debugging

### Issue 3: Webhook Called but Payment Not Found
**Symptoms:** Logs show `Payment not found for OrderCode: 123456`

**Solution:**
1. ✅ **FIXED**: Check `CheckoutController.cs` uses `createResult.orderCode`
2. Query database: `SELECT * FROM Payments WHERE OrderCode = 123456`
3. If not found: Payment creation failed → Check logs for errors

### Issue 4: Webhook Called but Status Not Updated
**Symptoms:** Logs show update attempted, but DB status still PENDING

**Solution:**
1. Check `PaymentService.UpdateStatusByOrderCodeAsync` calls `_paymentRepo.Update(payment)`
2. Check `IRepository` interface has `Update(T entity)` method
3. Check `RepositoryBase` implements `Update()`: 
   ```csharp
   public void Update(T entity) { _dbSet.Update(entity); }
   ```
4. Verify `_uow.CompleteAsync()` calls `SaveChangesAsync()`

---

## 📊 KEY METRICS TO MONITOR

### 1. **Payment Success Rate**
```sql
SELECT 
    Status,
    COUNT(*) as Count,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as Percentage
FROM Payments
WHERE DateCreated >= DATEADD(day, -7, GETDATE())
GROUP BY Status;
```

**Target:** 
- Paid: > 80%
- Pending: < 10%
- Failed: < 10%

### 2. **Webhook Response Time**
Check logs for time between:
- Payment created (CheckoutController)
- Webhook received (PayOSController)
- Status updated (PaymentService)

**Target:** < 10 seconds

### 3. **OrderCode Match Rate**
```sql
-- Check for orphaned payments (Pending > 1 hour)
SELECT 
    OrderCode,
    Amount,
    Status,
    DateCreated,
    DATEDIFF(minute, DateCreated, GETDATE()) as MinutesPending
FROM Payments
WHERE Status = 'Pending'
  AND DateCreated < DATEADD(hour, -1, GETDATE());
```

**Target:** 0 orphaned payments

---

## 🚀 DEPLOYMENT CHECKLIST

Before deploying fixes:
- [ ] ✅ Fixed: CheckoutController uses `createResult.orderCode`
- [ ] ✅ Added: IRepository.Update() method
- [ ] ✅ Added: RepositoryBase.Update() implementation
- [ ] ✅ Fixed: PaymentService calls `_paymentRepo.Update()`
- [ ] ✅ Enhanced: Webhook logging with emojis
- [ ] ✅ Verified: appsettings.json has correct PayOS keys
- [ ] Verified: Webhook URL configured on PayOS dashboard
- [ ] Tested: End-to-end payment flow in staging
- [ ] Monitored: Logs for webhook calls after deployment

---

## 📞 SUPPORT CONTACTS

**PayOS Support:**
- Website: https://payos.vn
- Email: support@payos.vn
- Docs: https://payos.vn/docs

**Azure Support:**
- Portal: https://portal.azure.com
- Logs: https://trippio.scm.azurewebsites.net

---

## ✅ EXPECTED BEHAVIOR AFTER FIX

1. **User adds items to cart** → Basket stored in Redis
2. **User clicks checkout** → `POST /api/Checkout/start`
   - Order created in DB
   - PayOS payment link created
   - **Payment saved with PayOS's OrderCode** ✅
   - Basket cleared
3. **User completes payment** → PayOS processes payment
4. **PayOS sends webhook** → `POST /api/payment/payos-callback`
   - Signature verified ✅
   - **OrderCode matches DB record** ✅
   - **Status updated Pending → Paid** ✅
5. **User sees success** → Payment complete!

**Result:** All payments should move from PENDING → PAID automatically! 🎉
