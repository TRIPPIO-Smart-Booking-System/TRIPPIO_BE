# 🔍 PayOS Payment API - Complete Check List

**Date:** November 11, 2025  
**Issue:** Signature verification failing, payments stuck at Pending

---

## 📋 API FLOW VERIFICATION

### 1. ✅ **POST /api/Checkout/start** - Checkout API

**What it does:**
```
Basket (Redis) 
  ↓
  Create Order (OrderId: 1, 2, 3...)
  ↓
  Clear Basket
  ↓
  Create PayOS payment link
  ↓
  Save Payment (OrderCode = OrderId)
  ↓
  Return checkout URL
```

**Expected Response:**
```json
{
  "code": 200,
  "message": "...",
  "data": {
    "checkoutUrl": "https://pay.payos.vn/...",
    "orderCode": 75,        // ← This should match OrderId!
    "amount": 10000,
    "status": "PENDING"
  }
}
```

**Logs to expect:**
```
Starting checkout for UserId: {UserId}
Basket retrieved: {ItemCount} items, Total: {Total} VND
Order created successfully: OrderId=75, Amount=10000
✅ Using OrderId as OrderCode: 75 for OrderId: 75
Creating PayOS payment link for OrderCode: 75
PayOS payment link created successfully. CheckoutUrl: https://...
✅ Checkout completed - OrderId: 75, OrderCode: 75
```

---

### 2. ✅ **POST /api/payment/payos-callback** - Webhook Handler

**What PayOS calls:**
```
User completes payment
  ↓
  PayOS verifies payment
  ↓
  PayOS sends webhook: POST /api/payment/payos-callback
  ↓
  Signature verified
  ↓
  Update Payment status: Pending → Paid
  ↓
  Update Order status: Pending → Confirmed
```

**Webhook Request Format (from PayOS):**
```json
{
  "code": "00",
  "data": {
    "orderCode": 75,           // ← MUST match OrderCode from Checkout
    "amount": 10000,
    "code": "00",              // Payment success
    "reference": "..."
  },
  "signature": "abc123..."    // HMAC-SHA256 signature
}
```

**Expected Logs:**
```
🔔 Webhook Raw Payload: OrderCode=75, Amount=10000, Code=00
🔐 Signature Attempts - Formula1: ..., Formula2: ..., Formula3: ...
💰 Payment SUCCESSFUL for OrderCode: 75, Amount: 10000
🔄 Starting payment status update for OrderCode: 75...
💾 Calling _paymentRepo.Update()...
✅ SUCCESS: Payment status updated to PAID for OrderCode: 75
```

---

## 🔧 CURRENT ISSUES

### ❌ Issue 1: Signature Mismatch

**Symptom:** 
```
Expected: ee238f3eb3b3a1dc1d0b5d9e4633f271015723f4b7ebd430faff194c247e191c
Got: 1d0e8be8851041b0c98321605512b1c0c9246219ecc3d7d21a376bf00849da8c
```

**Cause:** Signature formula or ChecksumKey mismatch

**Solution:** Use test endpoint to debug

---

## 🧪 TEST PROCEDURE

### Step 1: Debug Signature Formula

```bash
POST https://trippio.azurewebsites.net/api/payment/test-signature
Content-Type: application/json

{
  "orderCode": 75,
  "amount": 10000,
  "code": "00",
  "reference": "ref123",
  "signature": "1d0e8be8851041b0c98321605512b1c0c9246219ecc3d7d21a376bf00849da8c"
}
```

**Expected Response:**
```json
{
  "checksumKey": "8babdef8d145850c9e2af2b7f2be7e5d340a8f46fe1756ed11b40ed0c7d010f1",
  "receivedSignature": "1d0e8be8851041b0c98321605512b1c0c9246219ecc3d7d21a376bf00849da8c",
  "formulas": [
    {
      "formula": "orderCode|amount|code|reference",
      "signatureData": "75|10000|00|ref123",
      "signature": "abc123..."  // ← Compare with receivedSignature
    },
    {
      "formula": "orderCode|amount|code",
      "signatureData": "75|10000|00",
      "signature": "def456..."  // ← Or this one?
    },
    ...
  ]
}
```

**Action:** Find which formula's signature matches `receivedSignature`

---

### Step 2: End-to-End Test

**1. Create Payment:**
```bash
POST https://trippio.azurewebsites.net/api/Checkout/start
Authorization: Bearer YOUR_JWT_TOKEN

{
  "buyerName": "Test User",
  "buyerPhone": "0123456789"
}
```

**2. Note the OrderCode from response**

**3. Complete payment in PayOS**

**4. Check Logs in Azure Portal**
- Navigate to: trippio.azurewebsites.net → Monitor → Log Stream
- Look for: 🔔 🔐 💰 ✅

**5. Verify Database**
```sql
SELECT Status FROM Payments WHERE OrderCode = {yourOrderCode};
```

Expected: `Status = Paid` (NOT Pending)

---

## ✅ CHECKLIST

**Backend API:**
- [ ] CheckoutController: Uses OrderId as OrderCode
- [ ] CheckoutController: Saves payment with correct OrderCode
- [ ] PaymentService: UpdateStatusByOrderCodeAsync has logging
- [ ] PaymentService: Calls _paymentRepo.Update()
- [ ] PayOSController: Webhook handler has comprehensive logging
- [ ] IRepository: Has Update(T entity) method
- [ ] RepositoryBase: Implements Update()

**PayOS Configuration:**
- [ ] ChecksumKey matches PayOS dashboard
- [ ] Webhook URL: https://trippio.azurewebsites.net/api/payment/payos-callback
- [ ] Webhook method: POST
- [ ] Webhook active: Yes

**Testing:**
- [ ] Checkout creates order with OrderCode
- [ ] Webhook is called (check logs)
- [ ] Signature matches one of the formulas
- [ ] Payment status updates to Paid in DB
- [ ] Order status updates to Confirmed

---

## 🚀 NEXT STEPS

1. **Deploy build** (already done)
2. **Test payment** and check logs for signature formula match
3. **Identify correct formula** from test-signature endpoint
4. **Update webhook handler** with correct formula
5. **Verify all payments update** from Pending to Paid

---

## 📞 DEBUGGING COMMANDS

**Check PayOS webhook was called:**
```bash
# Azure Portal → Log Stream
# Search for: "🔔 PayOS Webhook Received"
```

**Check payment record:**
```sql
SELECT 
    OrderCode,
    Amount,
    Status,
    PaymentMethod,
    DateCreated,
    ModifiedDate
FROM Payments
WHERE OrderCode = 75;
```

**Check order status:**
```sql
SELECT 
    Id,
    Status,
    TotalAmount,
    OrderDate
FROM Orders
WHERE Id = 75;
```

---

## 💡 KEY INSIGHTS

1. **OrderCode = OrderId** (sequential: 1, 2, 3...)
2. **Signature formula** is probably different from what we assumed
3. **Webhook is being called** (logs show it), but signature fails
4. **Temporary bypass** allows testing - once formula is found, enable strict verification

---

**Status:** 🟡 In Testing Phase - Signature formula to be confirmed
