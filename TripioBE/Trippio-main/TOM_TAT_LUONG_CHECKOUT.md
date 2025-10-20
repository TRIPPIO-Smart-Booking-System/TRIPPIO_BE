# ✨ Tóm tắt: Luồng Checkout với PayOS

## 🎯 Câu hỏi ban đầu
> "Tôi chưa hiểu luồng nó sẽ đi như nào, thêm giỏ hàng vào Redis để cache rồi nó sẽ tạo order code như nào để payment?"

## 💡 Giải đáp

### **Luồng hoạt động (5 bước chính):**

```
1. ADD TO BASKET (Redis)
   ↓
2. CHECKOUT START
   ├─ Get Basket from Redis
   ├─ Create Order (DB) → Order.Id = 123
   ├─ Clear Basket (Redis)
   └─ Generate OrderCode = 123 (dùng Order.Id)
   ↓
3. CREATE PAYOS PAYMENT
   ├─ Call PayOS API với OrderCode=123
   └─ Return CheckoutUrl + QR Code
   ↓
4. USER PAYS
   User → PayOS Payment Page → Pay
   ↓
5. WEBHOOK CALLBACK
   PayOS → Backend → Update Payment + Order Status
```

### **Chi tiết từng bước:**

#### **Bước 1: Thêm giỏ hàng vào Redis**
```javascript
POST /api/basket/{userId}/items
Body: { productId, productName, price, quantity, productType }

→ Redis lưu: Key = userId, Value = Basket Object
→ Giỏ hàng tồn tại trong session user
```

#### **Bước 2: Bắt đầu Checkout**
```javascript
POST /api/checkout/start
Body: { buyerName, buyerEmail, buyerPhone }

Backend thực hiện:
1. Get basket từ Redis
2. Validate (không rỗng, >= 2000 VND)
3. Tạo Order mới trong DB:
   Order {
     Id: 123 (auto-increment),
     UserId: user-guid,
     TotalAmount: 5000000,
     Status: "Pending"
   }
4. Xóa basket khỏi Redis
5. Tạo OrderCode = Order.Id = 123
```

#### **Bước 3: Tạo OrderCode và PayOS Payment**
```javascript
// OrderCode = Order.Id
long orderCode = order.Id; // 123

// Nếu > 999999 (PayOS limit 6 chữ số)
if (orderCode > 999999) {
    orderCode = orderCode % 1000000;
}

// Call PayOS API
PaymentData {
  orderCode: 123,
  amount: 5000000,
  description: "Payment for Order #123",
  returnUrl: "https://yoursite.com/success",
  cancelUrl: "https://yoursite.com/cancel"
}

// PayOS trả về
{
  checkoutUrl: "https://pay.payos.vn/...",
  qrCode: "data:image/png;base64...",
  paymentLinkId: "pl_abc123",
  orderCode: 123
}
```

#### **Bước 4: User thanh toán**
```javascript
// Frontend redirect user đến checkoutUrl
window.location.href = checkoutUrl;

// Hoặc hiển thị QR code để quét
<img src={qrCode} alt="Scan to pay" />

// User chọn phương thức: ATM, Visa, Momo, v.v.
// PayOS xử lý giao dịch
```

#### **Bước 5: Webhook cập nhật status**
```javascript
// PayOS tự động gọi endpoint này khi thanh toán xong
POST /api/payment/payos-callback
Body: {
  data: {
    orderCode: 123,
    code: "00", // "00" = success
    amount: 5000000,
    reference: "FT123456789"
  }
}

Backend xử lý:
if (code == "00") {
  // ✅ Thành công
  - Update Payment.Status = "PAID"
  - Update Order.Status = "Confirmed"
  - Send email confirmation
} else {
  // ❌ Thất bại
  - Update Payment.Status = "FAILED"
  - Update Order.Status = "Cancelled"
}
```

## 🔑 Điểm quan trọng

### **1. OrderCode từ đâu?**
- **OrderCode = Order.Id** (ID tự động tăng từ database)
- Ví dụ: Order.Id = 123 → OrderCode = 123
- PayOS yêu cầu OrderCode là số long, max 6 chữ số (1-999999)

### **2. Tại sao dùng Redis cho Basket?**
- **Tốc độ:** Truy cập nhanh, không cần query DB
- **Tạm thời:** Giỏ hàng không cần lưu lâu dài
- **Giảm tải DB:** Mỗi thao tác (thêm/xóa/sửa) đều rất nhanh

### **3. Khi nào tạo Order?**
- **Không phải khi add to basket** (giỏ hàng chưa chốt)
- **Khi user click "Thanh toán"** (checkout/start)
- **Sau khi tạo Order** → Xóa basket → Tạo PayOS link

### **4. Luồng dữ liệu:**
```
Basket (Redis - Temp)
  ↓ checkout
Order (DB - Persistent)
  ↓ payment
Payment (DB - Persistent)
  ↓ webhook
Order.Status = Confirmed ✅
```

## 📊 So sánh với VNPay cũ

| Aspect | VNPay (Cũ) | PayOS (Mới) |
|--------|-----------|-------------|
| **SDK** | Custom implementation | Net.payOS NuGet package |
| **OrderCode** | Payment.Id (Guid) | Order.Id (int) |
| **Endpoint** | `/payment/vnpay` | `/checkout/start` |
| **Webhook** | Manual parsing | Strongly typed |
| **Integration** | Separate controller | Integrated in Checkout |
| **User Flow** | Redirect only | Redirect + QR code |

## 🎁 Files được tạo

1. **CHECKOUT_PAYOS_FLOW.md** - Luồng chi tiết, sơ đồ, FAQ
2. **API_EXAMPLES_CHECKOUT.md** - Code examples (JS, React, cURL)
3. **Trippio_PayOS_Checkout.postman_collection.json** - Postman collection
4. **PAYOS_INTEGRATION_SUMMARY.md** - Quick start guide

## ✅ Checklist hoàn thiện

### Đã làm xong:
- ✅ Tích hợp PayOS vào CheckoutController
- ✅ Sử dụng Order.Id làm OrderCode
- ✅ Xóa dependency VNPay khỏi checkout
- ✅ Thêm endpoints: start, status, cancel
- ✅ Documentation đầy đủ
- ✅ Postman collection để test

### Cần làm tiếp (TODO):
- ⬜ Extend Payment entity (thêm PaymentLinkId, OrderCode)
- ⬜ Implement webhook handler đầy đủ (update DB, send email)
- ⬜ Signature verification cho webhook
- ⬜ Auto-cancel expired orders
- ⬜ Unit tests
- ⬜ Integration tests với PayOS sandbox

## 🚀 Quick Start

### 1. Cài đặt dependencies
```bash
cd src/Trippio.Api
dotnet add package Net.payOS
```

### 2. Cấu hình appsettings.json
```json
{
  "PayOSSettings": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key",
    "ReturnUrl": "http://localhost:3000/payment-success",
    "CancelUrl": "http://localhost:3000/payment-cancel"
  }
}
```

### 3. Test API
```bash
# Import Postman collection
Trippio_PayOS_Checkout.postman_collection.json

# Run "Complete Flow - Happy Path" folder
```

### 4. Test Webhook (với ngrok)
```bash
# Terminal 1
dotnet run

# Terminal 2
ngrok http https://localhost:5001

# Update PayOS dashboard webhook:
# https://abc123.ngrok.io/api/payment/payos-callback
```

## 🎓 Kết luận

**Giờ bạn đã hiểu:**
1. ✅ Giỏ hàng được lưu trong **Redis** (cache tạm thời)
2. ✅ Khi checkout → Tạo **Order** trong DB (persistent)
3. ✅ **OrderCode** = **Order.Id** (ID tự động tăng)
4. ✅ PayOS nhận OrderCode để tạo payment link
5. ✅ Webhook cập nhật status khi thanh toán xong

**Luồng đơn giản:**
```
Basket → Order → OrderCode → PayOS → Webhook → Done ✅
```

Có câu hỏi gì khác không? 😊
