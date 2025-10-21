# 🛒 Payment Flow Test với User ID: 09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f

## 📋 Flow Thanh Toán Hoàn Chỉnh

```
Basket (Redis) → Order (DB) → PayOS Payment Link → Payment Record (DB) → Webhook Update
```

---

## 🧪 Test Scripts (Postman/REST Client)

### 1️⃣ **Add Items to Basket**

**Endpoint:** `POST /api/basket/{userId}/items`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "productId": "hotel-001",
  "quantity": 2,
  "price": 5000
}
```

**Curl Command:**
```bash
curl -X POST "http://localhost:7142/api/basket/09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f/items" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "hotel-001",
    "quantity": 2,
    "price": 5000
  }'
```

**Expected Response:**
```json
{
  "code": 200,
  "message": "Item added to basket successfully",
  "data": {
    "userId": "09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f",
    "items": [
      {
        "productId": "hotel-001",
        "quantity": 2,
        "price": 5000
      }
    ],
    "total": 10000
  }
}
```

---

### 2️⃣ **Add More Items (Optional)**

```json
{
  "productId": "transport-001",
  "quantity": 1,
  "price": 3000
}
```

```json
{
  "productId": "show-001",
  "quantity": 1,
  "price": 2000
}
```

---

### 3️⃣ **Get Basket**

**Endpoint:** `GET /api/basket/{userId}`

**Curl:**
```bash
curl -X GET "http://localhost:7142/api/basket/09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected Response:**
```json
{
  "code": 200,
  "message": "Basket retrieved successfully",
  "data": {
    "userId": "09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f",
    "items": [
      {
        "productId": "hotel-001",
        "quantity": 2,
        "price": 5000
      },
      {
        "productId": "transport-001",
        "quantity": 1,
        "price": 3000
      },
      {
        "productId": "show-001",
        "quantity": 1,
        "price": 2000
      }
    ],
    "total": 15000
  }
}
```

---

### 4️⃣ **Checkout - Create Order & Payment Link**

**Endpoint:** `POST /api/checkout/start`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "buyerName": "Nguyen Van A",
  "buyerEmail": "test@example.com",
  "buyerPhone": "0123456789"
}
```

**Curl:**
```bash
curl -X POST "http://localhost:7142/api/checkout/start" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "buyerName": "Nguyen Van A",
    "buyerEmail": "test@example.com",
    "buyerPhone": "0123456789"
  }'
```

**Expected Response:**
```json
{
  "code": 200,
  "message": "Order #123 created successfully. Please complete payment.",
  "data": {
    "checkoutUrl": "https://pay.payos.vn/web/abc123",
    "orderCode": 123,
    "amount": 15000,
    "qrCode": "https://api.payos.vn/qr/abc123.png",
    "paymentLinkId": "abc123",
    "status": "PENDING"
  }
}
```

**Quan trọng:** Sau step này, basket sẽ bị clear và payment record sẽ được tạo trong DB.

---

### 5️⃣ **Check Payment History**

**Endpoint:** `GET /api/payment/all`

**Curl:**
```bash
curl -X GET "http://localhost:7142/api/payment/all" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected Response (sau checkout):**
```json
{
  "code": 200,
  "message": "Payments retrieved successfully",
  "data": [
    {
      "id": "payment-guid-here",
      "userId": "09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f",
      "orderId": 123,
      "amount": 15000,
      "paymentMethod": "PayOS",
      "status": "Pending",
      "paidAt": "2025-10-21T10:00:00Z",
      "paymentLinkId": "abc123",
      "orderCode": 123
    }
  ]
}
```

---

### 6️⃣ **Simulate Payment Success (Webhook)**

**Endpoint:** `POST /api/payment/payos-callback`

**Request Body (thay orderCode bằng giá trị từ step 4):**
```json
{
  "data": {
    "orderCode": 123,
    "amount": 15000,
    "code": "00",
    "desc": "success",
    "reference": "FT123456789",
    "transactionDateTime": "2025-10-21 10:30:00",
    "description": "Payment for Order #123"
  }
}
```

**Curl:**
```bash
curl -X POST "http://localhost:7142/api/payment/payos-callback" \
  -H "Content-Type: application/json" \
  -d '{
    "data": {
      "orderCode": 123,
      "amount": 15000,
      "code": "00",
      "desc": "success",
      "reference": "FT123456789",
      "transactionDateTime": "2025-10-21 10:30:00",
      "description": "Payment for Order #123"
    }
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Webhook processed successfully"
}
```

---

### 7️⃣ **Verify Payment Status Updated**

**Check lại payment history:**
```bash
curl -X GET "http://localhost:7142/api/payment/all" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected Response (sau webhook):**
```json
{
  "code": 200,
  "data": [
    {
      "id": "payment-guid-here",
      "userId": "09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f",
      "orderId": 123,
      "amount": 15000,
      "paymentMethod": "PayOS",
      "status": "Paid",  // ✅ Updated!
      "paidAt": "2025-10-21T10:30:00Z",
      "paymentLinkId": "abc123",
      "orderCode": 123
    }
  ]
}
```

---

## 🔄 **Full Test Flow Script (PowerShell)**

```powershell
# 1. Add items to basket
curl -X POST "http://localhost:7142/api/basket/09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f/items" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN" `
  -H "Content-Type: application/json" `
  -d '{"productId": "hotel-001", "quantity": 2, "price": 5000}'

# 2. Add more items
curl -X POST "http://localhost:7142/api/basket/09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f/items" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN" `
  -H "Content-Type: application/json" `
  -d '{"productId": "transport-001", "quantity": 1, "price": 3000}'

# 3. Get basket
curl -X GET "http://localhost:7142/api/basket/09b1a4f5-335d-4cd8-88ea-7c8b4a149d9f" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# 4. Checkout
curl -X POST "http://localhost:7142/api/checkout/start" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN" `
  -H "Content-Type: application/json" `
  -d '{"buyerName": "Nguyen Van A", "buyerEmail": "test@example.com", "buyerPhone": "0123456789"}'

# 5. Check payment history (should have 1 record with status "Pending")
curl -X GET "http://localhost:7142/api/payment/all" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# 6. Simulate webhook (replace orderCode with actual value from step 4)
curl -X POST "http://localhost:7142/api/payment/payos-callback" `
  -H "Content-Type: application/json" `
  -d '{"data": {"orderCode": 123, "amount": 15000, "code": "00", "desc": "success", "reference": "FT123456789", "transactionDateTime": "2025-10-21 10:30:00"}}'

# 7. Check payment history again (status should be "Paid")
curl -X GET "http://localhost:7142/api/payment/all" `
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 📝 **Notes**

1. **JWT Token**: Bạn cần login trước để lấy JWT token cho Authorization header.
2. **Product IDs**: Sử dụng bất kỳ string nào cho productId (hotel-001, transport-001, etc.).
3. **Prices**: Đảm bảo tổng >= 2000 VND (yêu cầu của PayOS).
4. **OrderCode**: Trong response của checkout, copy orderCode để dùng trong webhook.
5. **Basket Clear**: Sau checkout thành công, basket sẽ bị clear tự động.
6. **Webhook**: Trong production, PayOS sẽ gọi webhook tự động khi user thanh toán.

---

## 🎯 **Expected Results**

- ✅ Basket có items trước checkout
- ✅ Checkout trả về PayOS payment link
- ✅ Payment record được tạo với status "Pending"
- ✅ Basket bị clear sau checkout
- ✅ Webhook update payment status thành "Paid"
- ✅ Order status update thành "Confirmed"
- ✅ `GET /api/payment/all` trả về đầy đủ lịch sử

**Nếu có lỗi, check logs API và database để debug!**
