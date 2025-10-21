# 🔧 Payment Flow Fix - Summary of Changes

## ✅ Vấn đề đã sửa

**Trước đây:**
- Checkout tạo PayOS payment link nhưng **KHÔNG lưu payment record vào DB**
- PayOS webhook nhận callback nhưng **KHÔNG update status trong DB**
- `GET /api/payment/all` trả về **rỗng** mặc dù đã thanh toán thành công

**Bây giờ:**
- ✅ Checkout tạo PayOS link **VÀ** lưu payment record vào DB với status "Pending"
- ✅ Webhook update status thành "Paid" hoặc "Failed" khi nhận callback từ PayOS
- ✅ `GET /api/payment/all` trả về **đầy đủ** lịch sử thanh toán

---

## 📝 Chi tiết các thay đổi

### 1. **Payment Entity** (`Trippio.Core/Domain/Entities/Payment.cs`)
Thêm 2 fields mới để lưu thông tin PayOS:
```csharp
public string? PaymentLinkId { get; set; }  // PayOS payment link ID
public long? OrderCode { get; set; }        // Order code (dùng để query)
```

### 2. **Payment Models** (`Trippio.Core/Models/Payment/PaymentModels.cs`)
- Extend `CreatePaymentRequest` với `PaymentLinkId` và `OrderCode`
- Extend `PaymentDto` để trả về PayOS data
- Thêm `UpdatePaymentStatusRequest` để update status

### 3. **IPaymentService** (`Trippio.Core/Services/IPaymentService.cs`)
Thêm 2 methods mới:
```csharp
Task<BaseResponse<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken ct);
Task<BaseResponse<PaymentDto>> UpdateStatusByOrderCodeAsync(long orderCode, string status, CancellationToken ct);
```

### 4. **PaymentService** (`Trippio.Data/Service/PaymentService.cs`)
Implement 2 methods:
- `CreateAsync`: Tạo payment record trong DB
- `UpdateStatusByOrderCodeAsync`: Update status theo OrderCode (dùng cho webhook)

### 5. **CheckoutController** (`Trippio.Api/Controllers/Checkout/CheckoutController.cs`)
**Step 5 - Lưu payment record:**
```csharp
var paymentRequest = new CreatePaymentRequest
{
    UserId = userId,
    OrderId = order.Id,
    Amount = order.TotalAmount,
    PaymentMethod = "PayOS",
    PaymentLinkId = createResult.paymentLinkId,
    OrderCode = orderCode
};

var paymentResponse = await _payments.CreateAsync(paymentRequest, ct);
```

### 6. **PayOSController** (`Trippio.Api/Controllers/Payment/PayOSController.cs`)
**Webhook - Update payment status:**
```csharp
if (code == "00")  // Payment successful
{
    await _paymentService.UpdateStatusByOrderCodeAsync(orderCode, "Paid");
}
else  // Payment failed
{
    await _paymentService.UpdateStatusByOrderCodeAsync(orderCode, "Failed");
}
```

---

## 🚀 Cách test

### 1. **Tạo migration và update database**
```bash
cd src/Trippio.Api
dotnet ef migrations add AddPayOSFieldsToPayment --project ../Trippio.Data --startup-project .
dotnet ef database update --project ../Trippio.Data --startup-project .
```

### 2. **Restart API**
```bash
docker compose down
docker compose up -d --build
```

### 3. **Test checkout flow**
```bash
# Step 1: Tạo order và payment link
POST /api/checkout/start
{
  "buyerName": "Test User",
  "buyerEmail": "test@example.com",
  "buyerPhone": "0123456789"
}

# Response sẽ có:
{
  "data": {
    "checkoutUrl": "https://pay.payos.vn/...",
    "orderCode": 123456,
    "paymentLinkId": "abc123"
  }
}

# Step 2: Check payment record đã được tạo
GET /api/payment/all

# Response nên thấy payment với status "Pending"
{
  "data": [
    {
      "id": "guid",
      "orderCode": 123456,
      "amount": 10000,
      "status": "Pending",
      "paymentLinkId": "abc123"
    }
  ]
}

# Step 3: Thanh toán qua PayOS link (hoặc simulate webhook)
# PayOS sẽ gọi webhook: POST /api/payment/payos-callback

# Step 4: Check lại payment status
GET /api/payment/all

# Status nên đã chuyển thành "Paid"
{
  "data": [
    {
      "id": "guid",
      "orderCode": 123456,
      "status": "Paid",  // ✅ Updated!
      "paidAt": "2025-10-21T10:30:00Z"
    }
  ]
}
```

### 4. **Test webhook manually (nếu PayOS chưa setup)**
```bash
POST /api/payment/payos-callback
Content-Type: application/json

{
  "data": {
    "orderCode": 123456,
    "amount": 10000,
    "code": "00",
    "desc": "success",
    "reference": "FT123456",
    "transactionDateTime": "2025-10-21 10:30:00"
  }
}
```

---

## 📊 Luồng hoàn chỉnh

```
┌─────────────┐
│   Basket    │ (Redis)
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Create Order│ (DB - orders table)
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ Create PayOS    │
│  Payment Link   │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Save Payment    │ ✅ FIXED: Lưu vào DB
│ Record (Pending)│    (payments table)
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ User Thanh toán │
│  qua PayOS      │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ PayOS Webhook   │
│   Callback      │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Update Payment  │ ✅ FIXED: Update status
│ Status to "Paid"│    (payments table)
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ Update Order    │
│ Status to       │
│ "Confirmed"     │
└─────────────────┘
```

---

## 🎯 Kết quả

- ✅ Payment records được lưu vào DB ngay khi tạo PayOS link
- ✅ Webhook update status tự động khi user thanh toán
- ✅ `GET /api/payment/all` trả về đầy đủ lịch sử
- ✅ Order status tự động update (Pending → Confirmed)
- ✅ Có thể track payment qua `OrderCode` hoặc `PaymentLinkId`

---

## ⚠️ Lưu ý

1. **Migration**: Phải chạy migration để thêm 2 columns mới (xem `MIGRATION_GUIDE.md`)
2. **Webhook URL**: Đăng ký webhook URL trên PayOS dashboard:
   ```
   https://your-domain.com/api/payment/payos-callback
   ```
3. **Testing**: Test webhook bằng ngrok hoặc deploy lên server public
4. **Security**: Webhook nên verify signature từ PayOS (hiện tại chưa implement, TODO)

---

**Người sửa:** AI Assistant  
**Ngày sửa:** 2025-10-21  
**Tham khảo:** Chat conversation về payment flow issues
