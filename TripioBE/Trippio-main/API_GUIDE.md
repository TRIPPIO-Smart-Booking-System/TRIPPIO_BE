# API HƯỚNG DẪN - TRIPPIO

## 1. BasketController
- **GET /api/basket/{userId}**: Lấy giỏ hàng của user
- **POST /api/basket/{userId}/items**: Thêm sản phẩm/dịch vụ vào giỏ
- **PUT /api/basket/{userId}/items/quantity**: Cập nhật số lượng sản phẩm trong giỏ
- **DELETE /api/basket/{userId}/items/{productId}**: Xóa sản phẩm khỏi giỏ
- **DELETE /api/basket/{userId}**: Xóa toàn bộ giỏ hàng

## 2. OrderController
- **GET /api/order/user/{userId}**: Lấy danh sách đơn hàng của user
- **GET /api/order/{id}**: Lấy chi tiết đơn hàng theo id
- **GET /api/order/status/{status}**: Lấy đơn hàng theo trạng thái
- **PUT /api/order/{id}/status?status=...**: Cập nhật trạng thái đơn hàng
- **PUT /api/order/{id}/cancel?userId=...**: Hủy đơn hàng
- **GET /api/order/revenue?from=...&to=...**: Tổng doanh thu theo khoảng thời gian
- **GET /api/order/pending**: Lấy đơn hàng chờ xử lý

## 3. CheckoutController
- **POST /api/checkout/start**: Bắt đầu quy trình checkout (tạo order từ basket, trả về paymentUrl)
  - Body: `{ "userId": "...", "paymentMethod": "..." }`

## 4. PaymentController
- **GET /api/payment/user/{userId}**: Lấy danh sách thanh toán của user
- **GET /api/payment/{id}**: Lấy chi tiết thanh toán
- **GET /api/payment/order/{orderId}**: Lấy thanh toán theo order
- **GET /api/payment/booking/{bookingId}**: Lấy thanh toán theo booking
- **PUT /api/payment/{id}/status?status=...**: Cập nhật trạng thái thanh toán
- **PUT /api/payment/{id}/refund?amount=...**: Hoàn tiền
- **GET /api/payment/total?from=...&to=...**: Tổng số tiền đã thanh toán

## 5. BookingController
- **GET /api/booking/{id}**: Lấy chi tiết booking
- **GET /api/booking/user/{userId}**: Lấy danh sách booking của user
- **GET /api/booking/status/{status}**: Lấy booking theo trạng thái
- **GET /api/booking/upcoming/{userId}**: Lấy booking sắp tới của user
- **PUT /api/booking/{id}/status?status=...**: Cập nhật trạng thái booking
- **PUT /api/booking/{id}/cancel?userId=...**: Hủy booking
- **GET /api/booking/total?from=...&to=...**: Tổng giá trị booking

## 6. Các controller khác (Hotel, Room, Transport, TransportTrip, Show...)
- Đã mô tả chi tiết trong file `IMPLEMENTATION_SUMMARY.md`

---

# LUỒNG CHẠY CHÍNH (FLOWCHART)

```mermaid
flowchart TD
    A[User chọn sản phẩm/dịch vụ] --> B[Thêm vào Basket (Redis)]
    B --> C[Tạo Order / Booking từ Basket]
    C --> D[Chọn phương thức thanh toán]
    D --> E[Gửi Payment Request đến Payment Controller]
    E --> F{Thanh toán thành công?}
    F -- Yes --> G[Cập nhật Payment Status = Paid]
    G --> H[Cập nhật Order/Booking Status = Confirmed]
    H --> I[Gửi Notification thành công tới User]
    I --> J[ScheduledJob xử lý hậu kỳ (ví dụ: gửi mail, in hóa đơn)]
    F -- No --> K[Payment Status = Failed]
    K --> L[Cập nhật Order/Booking Status = Cancelled]
    L --> I2[Gửi Notification thất bại tới User]
```

---

# GHI CHÚ CHO FE & HƯỚNG DẪN DOCKER

**Chạy terminal trong /src nha mấy cu**

1. Pull code mới backend
2. `docker volume ls`
3. `docker volume rm <tên volume còn cache>`
4. `docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build --remove-orphans`
5. Bấm vào localhost của Trippio API `/swagger` để test API

**Lưu ý:**
- Nếu lỗi volume, xóa volume cũ rồi chạy lại bước 4
- Nếu cần reset data, xóa volume và khởi động lại
- Swagger UI: http://localhost:5000/swagger (hoặc port bạn config)
