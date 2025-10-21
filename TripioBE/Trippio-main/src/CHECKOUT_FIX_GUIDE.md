# 🔧 Fix lỗi "Đơn thanh toán đã tồn tại" trong Checkout

## ❌ **Vấn đề:**
Khi checkout nhiều lần, PayOS trả về lỗi:
```json
{
  "code": 500,
  "message": "Checkout failed: Đơn thanh toán đã tồn tại",
  "data": null
}
```

**Nguyên nhân:** OrderCode trùng lặp với đơn thanh toán đã tạo trước đó trong PayOS.

## ✅ **Giải pháp đã implement:**

### 1. **Tạo OrderCode Unique**
Thay vì dùng `order.Id` (có thể trùng), giờ tạo OrderCode từ:
- **Timestamp** (Unix seconds) + **Random number** (3 digits)
- Đảm bảo 6 chữ số theo yêu cầu PayOS
- Rất khó trùng lặp

```csharp
// Ví dụ OrderCode được tạo: 123456, 234567, 345678, etc.
// Thay vì dùng order.Id có thể trùng
```

### 2. **Retry Logic**
Nếu vẫn bị conflict (hiếm), tự động retry với OrderCode mới:
- Thử tối đa 3 lần
- Mỗi lần tạo OrderCode mới
- Delay 100ms giữa các lần retry

### 3. **Logging cải thiện**
- Log OrderCode được tạo
- Log số lần retry
- Log lỗi chi tiết

## 🧪 **Cách test:**

### **Test Case 1: Checkout nhiều lần liên tiếp**
```bash
# Checkout lần 1
curl -X POST "http://localhost:7142/api/checkout/start" \
  -H "Authorization: Bearer YOUR_JWT" \
  -H "Content-Type: application/json" \
  -d '{"buyerName": "Test User", "buyerEmail": "test@example.com"}'

# Checkout lần 2 (sẽ có OrderCode khác)
curl -X POST "http://localhost:7142/api/checkout/start" \
  -H "Authorization: Bearer YOUR_JWT" \
  -H "Content-Type: application/json" \
  -d '{"buyerName": "Test User 2", "buyerEmail": "test2@example.com"}'

# Checkout lần 3 (vẫn khác)
curl -X POST "http://localhost:7142/api/checkout/start" \
  -H "Authorization: Bearer YOUR_JWT" \
  -H "Content-Type: application/json" \
  -d '{"buyerName": "Test User 3", "buyerEmail": "test3@example.com"}'
```

**Expected:** Tất cả checkout đều thành công với OrderCode khác nhau.

### **Test Case 2: Check Payment History**
```bash
curl -X GET "http://localhost:7142/api/payment/all" \
  -H "Authorization: Bearer YOUR_JWT"
```

**Expected:** 3 payment records với OrderCode khác nhau, status "Pending".

### **Test Case 3: Simulate Webhook**
```bash
# Lấy OrderCode từ response checkout
curl -X POST "http://localhost:7142/api/payment/payos-callback" \
  -H "Content-Type: application/json" \
  -d '{
    "data": {
      "orderCode": ORDER_CODE_FROM_CHECKOUT,
      "amount": 15000,
      "code": "00",
      "desc": "success"
    }
  }'
```

## 📊 **Kết quả mong đợi:**

✅ **Không còn lỗi "Đơn thanh toán đã tồn tại"**
✅ **Mỗi checkout có OrderCode unique**
✅ **Payment records lưu đúng trong DB**
✅ **Webhook update status thành công**

## 🔍 **Debug nếu vẫn lỗi:**

### Check logs API:
```bash
docker logs -f trippio-api
```

**Tìm log như:**
```
Generated unique OrderCode: 123456 for OrderId: 123
PayOS payment link created successfully. CheckoutUrl: https://pay.payos.vn/...
```

### Nếu retry xảy ra:
```
PayOS payment creation failed (attempt 1), retrying with new OrderCode: 234567
```

### Check PayOS dashboard:
- Xem các đơn thanh toán đã tạo
- Verify OrderCode không trùng

## 🚀 **Deploy:**

1. **Restart API:**
```bash
docker compose down
docker compose up -d --build
```

2. **Test lại flow checkout**

---

**Fix này đảm bảo checkout luôn thành công với OrderCode unique, không còn conflict với PayOS! 🎉**
