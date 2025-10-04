# LUỒNG CHẠY CHÍNH - TRIPPIO

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

> Xem thêm chi tiết API và hướng dẫn sử dụng trong file `API_GUIDE.md` và `IMPLEMENTATION_SUMMARY.md`.