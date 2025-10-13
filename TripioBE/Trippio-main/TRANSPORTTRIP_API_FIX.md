# TransportTrip API - Swagger POST Guide

## Lỗi đã được khắc phục

### 1. **Phân tích lỗi gốc**
- **Lỗi 500**: Xuất hiện do EF Core tracking conflict khi gửi nested Transport object trong TransportTrip
- **Nguyên nhân**: Swagger tự động generate nested object, khiến EF Core bị confused về entity tracking
- **Vị trí lỗi**: Service layer chưa xử lý navigation properties đúng cách

### 2. **Hướng khắc phục**
- ✅ Tạo **CreateTransportTripRequest DTO** để tránh nested object
- ✅ Thêm validation trong **TransportTripService** 
- ✅ Cấu hình **JSON serialization** tránh circular reference
- ✅ Thêm error handling chi tiết trong service layer

### 3. **Cách POST đúng trên Swagger**

#### ✅ **Body Request mới (Khuyến nghị)**
```json
{
  "transportId": "550e8400-e29b-41d4-a716-446655440001",
  "departure": "Ho Chi Minh City",
  "destination": "Da Nang",
  "departureTime": "2024-12-15T08:00:00Z",
  "arrivalTime": "2024-12-15T10:30:00Z",
  "price": 150000,
  "availableSeats": 50
}
```

#### ❌ **Body cũ (Gây lỗi 500)**
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "transportId": "550e8400-e29b-41d4-a716-446655440001",
  "departure": "Ho Chi Minh City",
  "destination": "Da Nang",
  "departureTime": "2024-12-15T08:00:00Z",
  "arrivalTime": "2024-12-15T10:30:00Z",
  "price": 150000,
  "availableSeats": 50,
  "transport": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "transportType": "Bus",
    "name": "Xe Giường Nằm VIP"
  }
}
```

## Code đã được cập nhật

### 1. **CreateTransportTripRequest.cs** (Mới)
- Chỉ chứa các field cần thiết
- Không có nested transport object
- Có validation attributes

### 2. **TransportTripService.cs** (Đã cập nhật)
```csharp
public async Task<TransportTrip> CreateTransportTripAsync(TransportTrip transportTrip)
{
    try
    {
        // Validate Transport exists
        var transportExists = await _context.Transports.AnyAsync(t => t.Id == transportTrip.TransportId);
        if (!transportExists)
        {
            throw new InvalidOperationException($"Transport with ID {transportTrip.TransportId} does not exist.");
        }

        // Clear navigation properties to avoid EF tracking issues
        transportTrip.Transport = null!;
        
        // Set audit fields
        transportTrip.DateCreated = DateTime.UtcNow;
        transportTrip.ModifiedDate = null;

        await _transportTripRepository.Add(transportTrip);
        await _unitOfWork.CompleteAsync();

        return transportTrip;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating transport trip");
        throw;
    }
}
```

### 3. **Program.cs** (JSON Configuration)
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
```

## Lợi ích sau khi fix

1. **✅ Không còn lỗi 500** khi POST TransportTrip
2. **✅ Swagger UI sạch sẽ** - không hiển thị nested object phức tạp
3. **✅ Validation tốt hơn** - kiểm tra Transport tồn tại trước khi tạo
4. **✅ Error handling rõ ràng** - message lỗi cụ thể cho user
5. **✅ Performance tốt hơn** - không load unnecessary navigation properties

## Test API

1. **Tạo Transport trước** (nếu chưa có):
```bash
POST /api/Transport
{
  "transportType": "Bus",
  "name": "Xe Giường Nằm VIP"
}
```

2. **Tạo TransportTrip** với transportId từ bước 1:
```bash
POST /api/TransportTrip
{
  "transportId": "{transport_id_from_step_1}",
  "departure": "Ho Chi Minh City",
  "destination": "Da Nang",
  "departureTime": "2024-12-15T08:00:00Z",
  "arrivalTime": "2024-12-15T10:30:00Z", 
  "price": 150000,
  "availableSeats": 50
}
```

**Response thành công (201 Created):**
```json
{
  "id": "new-generated-guid",
  "transportId": "550e8400-e29b-41d4-a716-446655440001",
  "departure": "Ho Chi Minh City",
  "destination": "Da Nang",
  "departureTime": "2024-12-15T08:00:00Z",
  "arrivalTime": "2024-12-15T10:30:00Z",
  "price": 150000,
  "availableSeats": 50,
  "dateCreated": "2024-10-13T10:30:00Z",
  "modifiedDate": null,
  "transportName": "Xe Giường Nằm VIP",
  "transportType": "Bus"
}
```