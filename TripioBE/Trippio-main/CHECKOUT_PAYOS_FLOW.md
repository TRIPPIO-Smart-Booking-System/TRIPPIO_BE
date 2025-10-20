# Luồng Checkout với PayOS Integration

## Tổng quan
Dự án đã được tích hợp PayOS vào `CheckoutController` thay thế VNPay. Order.Id được sử dụng làm OrderCode cho PayOS để đồng nhất và dễ tracking.

## Luồng hoạt động chi tiết

### 1. **Thêm sản phẩm vào giỏ hàng (Basket - Redis)**

**Endpoint:** `POST /api/basket/{userId}/items`

```json
{
  "productId": "hotel-123",
  "productName": "Deluxe Room",
  "price": 1500000,
  "quantity": 2,
  "productType": "Hotel"
}
```

**Xử lý:**
- Item được thêm vào giỏ hàng và lưu trong **Redis** (cache) với key = `userId`
- Redis giúp truy cập nhanh và giảm tải database
- Giỏ hàng tồn tại trong phiên làm việc của user

**Các thao tác khác:**
- `GET /api/basket/{userId}` - Xem giỏ hàng
- `PUT /api/basket/{userId}/items/quantity` - Cập nhật số lượng
- `DELETE /api/basket/{userId}/items/{productId}` - Xóa item
- `DELETE /api/basket/{userId}` - Xóa toàn bộ giỏ

---

### 2. **Bắt đầu Checkout (Tạo Order + PayOS Payment)**

**Endpoint:** `POST /api/checkout/start`

**Request:**
```json
{
  "userId": "user-guid-here", // Optional, sẽ lấy từ JWT token nếu không có
  "buyerName": "Nguyễn Văn A",
  "buyerEmail": "example@email.com",
  "buyerPhone": "0123456789"
}
```

**Luồng xử lý (Step by step):**

#### **Step 1: Xác thực người dùng**
- Lấy `userId` từ JWT token (Claims: `NameIdentifier` hoặc `sub`)
- Nếu không có token → Return `401 Unauthorized`

#### **Step 2: Lấy giỏ hàng từ Redis**
- Gọi `IBasketService.GetAsync(userId)`
- Kiểm tra giỏ hàng có tồn tại và không rỗng
- Kiểm tra tổng tiền >= 2000 VND (yêu cầu tối thiểu của PayOS)
- Nếu không hợp lệ → Return `400 Bad Request`

**Log:** 
```
Basket retrieved: 3 items, Total: 4500000 VND
```

#### **Step 3: Tạo Order từ Basket**
- Gọi `IOrderService.CreateFromBasketAsync(userId, basket)`
- Tạo entity `Order` mới:
  ```csharp
  Order {
    Id: 123 (auto-increment từ DB),
    UserId: user-guid,
    TotalAmount: 4500000,
    Status: OrderStatus.Pending,
    OrderDate: DateTime.UtcNow
  }
  ```
- Lưu vào database (SQL Server)
- Trả về `OrderDto`

**Log:**
```
Order created successfully: OrderId=123, Amount=4500000
```

#### **Step 4: Xóa Basket khỏi Redis**
- Gọi `IBasketService.ClearAsync(userId)`
- Xóa cache để tránh duplicate order
- User phải thêm lại nếu muốn mua tiếp

**Log:**
```
Basket cleared for UserId: user-guid
```

#### **Step 5: Tạo OrderCode cho PayOS**
- **OrderCode = Order.Id** (ví dụ: 123)
- PayOS yêu cầu OrderCode là số long trong khoảng 1-999999 (max 6 chữ số)
- Nếu `Order.Id > 999999`, sử dụng `Order.Id % 1000000` để fit

**Code:**
```csharp
long orderCode = order.Id;
if (orderCode > 999999) {
    orderCode = orderCode % 1000000; // Fallback
}
```

**Lưu ý:** 
- Nếu dự án của bạn có nhiều order (>999999), cần xem xét thiết kế lại:
  - Option 1: Reset Order.Id sequence định kỳ
  - Option 2: Tạo bảng riêng `PaymentTransaction` với ID riêng
  - Option 3: Sử dụng timestamp-based code

#### **Step 6: Chuẩn bị dữ liệu PayOS**
- Tạo `PaymentData` với các thông tin:
  ```csharp
  PaymentData {
    orderCode: 123,
    amount: 4500000,
    description: "Payment for Order #123",
    items: [
      { name: "Order #123 - 3 items", quantity: 1, price: 4500000 }
    ],
    cancelUrl: "http://localhost:3000/payment-cancel",
    returnUrl: "http://localhost:3000/payment-success",
    buyerName: "Nguyễn Văn A",
    buyerEmail: "example@email.com",
    buyerPhone: "0123456789"
  }
  ```

#### **Step 7: Gọi PayOS API**
- Sử dụng PayOS SDK: `_payOS.createPaymentLink(paymentData)`
- PayOS trả về:
  ```csharp
  CreatePaymentResult {
    checkoutUrl: "https://pay.payos.vn/payment/...",
    qrCode: "data:image/png;base64,...",
    paymentLinkId: "payment-link-id",
    orderCode: 123
  }
  ```

**Log:**
```
PayOS payment link created successfully. CheckoutUrl: https://pay.payos.vn/...
```

#### **Step 8: Lưu Payment record (Optional - TODO)**
- **Hiện tại chưa implement đầy đủ** (có TODO trong code)
- Nên tạo record trong bảng `Payments`:
  ```csharp
  Payment {
    Id: new Guid(),
    UserId: user-guid,
    OrderId: 123,
    Amount: 4500000,
    PaymentMethod: "PayOS",
    Status: PaymentStatus.Pending,
    PaymentLinkId: "payment-link-id",
    OrderCode: 123,
    DateCreated: DateTime.UtcNow
  }
  ```

#### **Step 9: Trả về Response**
**Response (200 OK):**
```json
{
  "code": 200,
  "message": "Order #123 created successfully. Please complete payment.",
  "data": {
    "checkoutUrl": "https://pay.payos.vn/payment/...",
    "orderCode": 123,
    "amount": 4500000,
    "qrCode": "data:image/png;base64,...",
    "paymentLinkId": "payment-link-id",
    "status": "PENDING"
  }
}
```

---

### 3. **User thanh toán qua PayOS**

- Frontend nhận `checkoutUrl` và redirect user đến trang thanh toán PayOS
- User quét QR code hoặc chọn phương thức thanh toán (ATM, Visa, Momo, v.v.)
- PayOS xử lý giao dịch

**Các kết quả:**
- ✅ **Thành công:** PayOS redirect về `returnUrl` với query params
- ❌ **Thất bại/Hủy:** PayOS redirect về `cancelUrl`

---

### 4. **PayOS Webhook - Cập nhật trạng thái thanh toán**

**Endpoint:** `POST /api/payment/payos-callback` (trong `PayOSController`)

**Webhook được gọi tự động bởi PayOS server khi:**
- Thanh toán thành công
- Thanh toán thất bại
- Thanh toán bị hủy

**Webhook Data:**
```json
{
  "data": {
    "orderCode": 123,
    "amount": 4500000,
    "code": "00", // "00" = success, khác = failed
    "desc": "Giao dịch thành công",
    "reference": "FT123456789",
    "transactionDateTime": "2025-10-20 14:30:00",
    "description": "Payment for Order #123"
  }
}
```

**Xử lý webhook:**
- Parse dữ liệu từ PayOS
- Kiểm tra `code`:
  - `"00"` → **THÀNH CÔNG**
    - Cập nhật `Payment.Status = "PAID"`
    - Cập nhật `Order.Status = "Confirmed"` hoặc `"Processing"`
    - Gửi email xác nhận cho user
    - Có thể cộng balance cho user (nếu có tính năng wallet)
  - Khác `"00"` → **THẤT BẠI/HỦY**
    - Cập nhật `Payment.Status = "FAILED"` hoặc `"CANCELLED"`
    - Cập nhật `Order.Status = "Cancelled"`
    - Thông báo cho user

**Log:**
```
Payment SUCCESSFUL for OrderCode: 123, Amount: 4500000
```

**TODO trong code:**
```csharp
// TODO: Update payment status in database
// await _paymentService.UpdatePaymentStatusAsync(orderCode.ToString(), "PAID");

// TODO: Update order status
// await _orders.UpdateStatusAsync((int)orderCode, "Confirmed");

// TODO: Send confirmation email
// await _emailService.SendPaymentConfirmationAsync(userId, orderCode);
```

---

### 5. **Các endpoint bổ sung**

#### **Kiểm tra trạng thái thanh toán**
**Endpoint:** `GET /api/checkout/status/{orderCode}`

```bash
GET /api/checkout/status/123
```

**Response:**
```json
{
  "code": 200,
  "message": "Payment information retrieved successfully",
  "data": {
    "orderCode": 123,
    "amount": 4500000,
    "status": "PAID", // hoặc "PENDING", "CANCELLED"
    "transactions": [
      {
        "reference": "FT123456789",
        "amount": 4500000,
        "description": "Payment for Order #123",
        "transactionDateTime": "2025-10-20 14:30:00"
      }
    ]
  }
}
```

#### **Hủy thanh toán**
**Endpoint:** `POST /api/checkout/cancel/{orderCode}`

```bash
POST /api/checkout/cancel/123
Content-Type: application/json

"User changed mind"
```

**Response:**
```json
{
  "code": 200,
  "message": "Checkout cancelled successfully",
  "data": {
    "orderCode": 123,
    "status": "CANCELLED"
  }
}
```

**Lưu ý:** Chỉ có thể hủy thanh toán khi status còn `PENDING`. Nếu đã `PAID` thì không thể hủy.

---

## Sơ đồ luồng (Flow Diagram)

```
┌─────────────┐
│   User      │
└──────┬──────┘
       │
       │ 1. Add items to basket
       ▼
┌─────────────────────┐
│  BasketController   │
│  (POST /basket)     │
└──────┬──────────────┘
       │
       │ Items saved to Redis
       ▼
┌─────────────────────┐
│     Redis Cache     │
│  Key: userId        │
│  Value: Basket{}    │
└──────┬──────────────┘
       │
       │ 2. Start checkout
       ▼
┌─────────────────────────┐
│  CheckoutController     │
│  (POST /checkout/start) │
└──────┬──────────────────┘
       │
       │ 2.1 Get basket from Redis
       │ 2.2 Create Order (DB)
       │ 2.3 Clear basket (Redis)
       │ 2.4 Generate OrderCode = Order.Id
       │ 2.5 Call PayOS API
       ▼
┌─────────────────────┐
│   PayOS Server      │
│  (createPaymentLink)│
└──────┬──────────────┘
       │
       │ Returns checkoutUrl + QR
       ▼
┌─────────────────────┐
│  Frontend           │
│  Redirect to PayOS  │
└──────┬──────────────┘
       │
       │ 3. User pays
       ▼
┌─────────────────────┐
│  PayOS Payment Page │
│  (Scan QR / Select) │
└──────┬──────────────┘
       │
       │ 4. Payment completed
       ▼
┌─────────────────────────┐
│  PayOS Webhook          │
│  (POST /payos-callback) │
└──────┬──────────────────┘
       │
       │ 4.1 Parse webhook data
       │ 4.2 Update Payment status
       │ 4.3 Update Order status
       │ 4.4 Send email
       ▼
┌─────────────────────┐
│   Database          │
│   Order: CONFIRMED  │
│   Payment: PAID     │
└─────────────────────┘
```

---

## Cấu hình PayOS

### File: `appsettings.json`
```json
{
  "PayOSSettings": {
    "ClientId": "your-client-id-from-payos-dashboard",
    "ApiKey": "your-api-key-from-payos-dashboard",
    "ChecksumKey": "your-checksum-key-from-payos-dashboard",
    "ReturnUrl": "http://localhost:3000/payment-success",
    "CancelUrl": "http://localhost:3000/payment-cancel"
  }
}
```

### Lấy credentials từ đâu?
1. Đăng ký/Đăng nhập: https://my.payos.vn
2. Tạo merchant/shop
3. Vào **Settings** → **API Keys**
4. Copy `Client ID`, `API Key`, `Checksum Key`

### Cấu hình Webhook URL
1. Vào PayOS Dashboard
2. Settings → **Webhook URL**
3. Nhập: `https://yourdomain.com/api/payment/payos-callback`
4. **Lưu ý:** Phải là HTTPS (production) hoặc dùng ngrok (development)

---

## TODO - Những việc cần hoàn thiện

### 1. **Extend Payment Entity**
Thêm các field cho PayOS:
```csharp
public class Payment
{
    // Existing fields...
    
    // New fields for PayOS
    public string? PaymentLinkId { get; set; }
    public long? OrderCode { get; set; }
    public string? PayOSReference { get; set; }
    public string? PayOSTransactionDateTime { get; set; }
}
```

### 2. **Implement Payment Service methods**
```csharp
Task UpdatePaymentStatusAsync(long orderCode, string status);
Task<PaymentDto?> GetPaymentByOrderCodeAsync(long orderCode);
```

### 3. **Update Order Status sau khi thanh toán**
```csharp
// In PayOSController webhook handler
if (code == "00") 
{
    await _orders.UpdateStatusAsync((int)orderCode, "Confirmed");
}
```

### 4. **Email Notification**
- Gửi email xác nhận khi thanh toán thành công
- Gửi email thông báo khi thanh toán thất bại

### 5. **Error Handling & Retry Logic**
- Xử lý trường hợp PayOS API timeout
- Retry mechanism cho webhook
- Idempotency cho webhook (tránh xử lý trùng)

### 6. **Testing**
- Unit test cho CheckoutController
- Integration test với PayOS sandbox
- Test webhook với ngrok

---

## Testing với Ngrok (Development)

PayOS webhook cần HTTPS public URL. Để test local:

```bash
# Install ngrok
choco install ngrok

# Start your API
dotnet run

# Expose local port
ngrok http 5000

# Copy HTTPS URL (e.g., https://abc123.ngrok.io)
# Update PayOS dashboard webhook: https://abc123.ngrok.io/api/payment/payos-callback
```

---

## Security Considerations

1. **Webhook Signature Verification**
   - PayOS gửi signature trong header
   - Cần verify để đảm bảo webhook thật từ PayOS
   - TODO: Implement signature validation

2. **HTTPS Only**
   - Production phải dùng HTTPS
   - PayOS reject HTTP webhook URLs

3. **Order Amount Validation**
   - Verify amount trong webhook match với DB
   - Tránh user manipulate amount

4. **Idempotency**
   - PayOS có thể gọi webhook nhiều lần
   - Cần check xem order đã được xử lý chưa

---

## FAQ

### Q: OrderCode có thể trùng không?
A: Không được trùng trong cùng PayOS merchant. Vì dùng `Order.Id` (auto-increment, unique) nên không trùng.

### Q: Nếu Order.Id vượt quá 999999 thì sao?
A: Có 3 giải pháp:
1. Reset sequence định kỳ
2. Dùng modulo (`Order.Id % 1000000`) - có risk trùng
3. Tạo bảng `PaymentTransaction` riêng với ID sequence riêng

### Q: User hủy payment thì Order bị xóa không?
A: Không. Order vẫn tồn tại với status `Pending`. Cần thêm logic để auto-cancel order sau X phút nếu không thanh toán.

### Q: Có thể dùng cả PayOS và VNPay không?
A: Có. Thêm field `PaymentMethod` trong request, if-else để chọn gateway phù hợp.

---

## Support

- PayOS Documentation: https://payos.vn/docs
- PayOS Dashboard: https://my.payos.vn
- PayOS Support: support@payos.vn
