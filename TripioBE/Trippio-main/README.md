# 🌍 Trippio Backend - ASP.NET Core Web API

Trippio Backend là một RESTful API service được xây dựng bằng ASP.NET Core, phục vụ cho nền tảng đặt vé du lịch trực tuyến. Hệ thống hỗ trợ quản lý khách sạn, vận chuyển, show/giải trí, đặt hàng, thanh toán và nhiều tính năng khác.

## 🚀 Công Nghệ Sử Dụng

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: Azure SQL Server
- **Authentication**: JWT Bearer Token, Google OAuth 2.0
- **Authorization**: Role-Based Access Control (RBAC) với Custom Policy-Based Authorization
- **Caching**: Redis (Azure Redis Cache)
- **Logging**: Serilog với file logging
- **API Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Payment Gateway**: VNPay, PayOS
- **Architecture Pattern**: Repository + Unit of Work Pattern
- **Mapping**: AutoMapper
- **Email Service**: SMTP
- **Containerization**: Docker & Docker Compose

## ✨ Tính Năng Chính

### Authentication & Authorization
- 🔐 JWT Token-based Authentication
- 🔑 Google OAuth 2.0 Integration
- 👥 Role-Based Access Control (Admin, Staff, Customer)
- 🛡️ Custom Permission-Based Authorization
- 🔄 Token Refresh Mechanism

### Core Business Features
- 🏨 **Hotel Management**: CRUD khách sạn và phòng, quản lý giá theo ngày
- 🚗 **Transport Management**: Quản lý phương tiện, tuyến đường, chuyến đi
- 🎭 **Show/Entertainment Management**: Quản lý show, sự kiện, vé
- 🛒 **Basket/Cart**: Giỏ hàng với Redis caching
- 📦 **Order Management**: Quản lý đơn hàng, tracking trạng thái
- 💳 **Payment Integration**: VNPay, PayOS với webhook support
- ⭐ **Review System**: Đánh giá và phản hồi dịch vụ
- 👤 **User Profile Management**: Quản lý thông tin người dùng

### Admin Features
- 📊 Dashboard với thống kê
- 👥 User Management
- 🏢 Service Management (Hotels, Transports, Shows)
- 📦 Order Processing
- 💰 Payment Management
- 📝 Review Moderation

### Technical Features
- 🔄 Idempotency Support cho các operations quan trọng
- 📄 Pagination Support
- 🔍 Filtering & Search
- 📊 Health Checks
- 📝 Comprehensive Logging
- 🐳 Docker Support
- 🔄 Database Migrations

## 📦 Cấu Trúc Dự Án

```
src/
├── Trippio.Api/                    # Web API Project
│   ├── Controllers/               # API Controllers
│   │   ├── AdminApi/             # Admin endpoints
│   │   ├── Auth/                 # Authentication endpoints
│   │   ├── Basket/               # Cart/Basket endpoints
│   │   ├── Booking/              # Booking endpoints
│   │   ├── Checkout/             # Checkout endpoints
│   │   ├── Order/                # Order management
│   │   ├── Payment/              # Payment processing
│   │   ├── HotelController.cs
│   │   ├── TransportController.cs
│   │   ├── ShowController.cs
│   │   └── ReviewController.cs
│   ├── Authorization/             # Custom authorization handlers
│   ├── Extensions/               # Extension methods
│   ├── Filters/                  # Action filters
│   ├── Idempotency/             # Idempotency implementation
│   ├── Service/                  # Application services
│   ├── Security/                 # Security utilities
│   ├── Program.cs               # Application entry point
│   └── appsettings.json         # Configuration
│
├── Trippio.Core/                  # Core Business Logic
│   ├── Domain/                   # Domain entities
│   │   ├── Identity/            # User, Role entities
│   │   ├── Content/             # Hotels, Rooms, Shows
│   │   └── Ordering/            # Orders, OrderItems
│   ├── Repositories/             # Repository interfaces & implementations
│   ├── Services/                 # Business services
│   ├── Models/                   # DTOs, View Models
│   ├── Mappings/                # AutoMapper profiles
│   └── SeedWorks/               # Base classes, interfaces
│
└── docker-compose.yml            # Docker orchestration
```

## 🛠️ Cài Đặt & Chạy Dự Án

### Yêu Cầu Hệ Thống
- .NET 8.0 SDK
- SQL Server 2019+ hoặc Azure SQL
- Redis Server (hoặc Azure Redis Cache)
- Visual Studio 2022 / VS Code / Rider
- Docker Desktop (optional)

### Cài Đặt

1. **Clone Repository**
```bash
git clone <repository-url>
cd Trippio-main/src
```

2. **Cấu Hình Database**

Tạo file `appsettings.Development.json` từ template và cập nhật connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TrippioDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True",
    "Redis": "localhost:6379"
  }
}
```

3. **Restore Dependencies**
```bash
dotnet restore
```

4. **Apply Migrations**
```bash
cd Trippio.Api
dotnet ef database update
```

5. **Chạy Application**

**Development Mode:**
```bash
# Windows
.\dev-start.bat

# Linux/Mac
./dev-start.sh
```

**Manual Run:**
```bash
dotnet run --project Trippio.Api
```

API sẽ chạy tại: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### Chạy với Docker

```bash
# Build và start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string",
    "Redis": "Redis connection string"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "Trippio.Api",
    "Audience": "Trippio.Client",
    "ExpiryMinutes": 60
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id"
  },
  "Payments": {
    "VNPay": {
      "TmnCode": "your-vnpay-tmncode",
      "HashSecret": "your-vnpay-hashsecret"
    },
    "PayOS": {
      "ClientId": "your-payos-client-id",
      "ApiKey": "your-payos-api-key",
      "ChecksumKey": "your-payos-checksum-key"
    }
  }
}
```

## 📚 API Documentation

### Authentication Endpoints

```
POST   /api/Auth/register          # Đăng ký tài khoản mới
POST   /api/Auth/login             # Đăng nhập
POST   /api/Auth/google-login      # Đăng nhập Google OAuth
POST   /api/Auth/refresh-token     # Refresh JWT token
POST   /api/Auth/logout            # Đăng xuất
```

### Hotel Endpoints

```
GET    /api/Hotel                  # Lấy danh sách khách sạn (pagination, filter)
GET    /api/Hotel/{id}             # Lấy chi tiết khách sạn
POST   /api/Hotel                  # Tạo khách sạn mới [Admin]
PUT    /api/Hotel/{id}             # Cập nhật khách sạn [Admin]
DELETE /api/Hotel/{id}             # Xóa khách sạn [Admin]
GET    /api/Hotel/{id}/rooms       # Lấy danh sách phòng của khách sạn
```

### Transport Endpoints

```
GET    /api/Transport              # Lấy danh sách phương tiện
GET    /api/Transport/{id}         # Chi tiết phương tiện
POST   /api/Transport              # Tạo phương tiện [Admin]
PUT    /api/Transport/{id}         # Cập nhật phương tiện [Admin]
DELETE /api/Transport/{id}         # Xóa phương tiện [Admin]
```

### Show Endpoints

```
GET    /api/Show                   # Lấy danh sách show/sự kiện
GET    /api/Show/{id}              # Chi tiết show
POST   /api/Show                   # Tạo show mới [Admin]
PUT    /api/Show/{id}              # Cập nhật show [Admin]
DELETE /api/Show/{id}              # Xóa show [Admin]
```

### Basket/Cart Endpoints

```
GET    /api/Basket                 # Lấy giỏ hàng hiện tại
POST   /api/Basket/items           # Thêm item vào giỏ
PUT    /api/Basket/items/{id}      # Cập nhật item
DELETE /api/Basket/items/{id}      # Xóa item
DELETE /api/Basket                 # Xóa toàn bộ giỏ hàng
```

### Order Endpoints

```
GET    /api/Order                  # Lấy danh sách đơn hàng
GET    /api/Order/{id}             # Chi tiết đơn hàng
POST   /api/Order                  # Tạo đơn hàng mới
PUT    /api/Order/{id}/status      # Cập nhật trạng thái [Admin/Staff]
```

### Payment Endpoints

```
POST   /api/Payment/vnpay/create           # Tạo payment VNPay
POST   /api/Payment/vnpay/callback         # VNPay webhook callback
POST   /api/Payment/payos/create           # Tạo payment PayOS
POST   /api/Payment/payos/webhook          # PayOS webhook
GET    /api/Payment/{orderId}              # Lấy thông tin payment
```

### Review Endpoints

```
GET    /api/Review/hotel/{hotelId}         # Lấy review của khách sạn
GET    /api/Review/show/{showId}           # Lấy review của show
POST   /api/Review                         # Tạo review mới
PUT    /api/Review/{id}                    # Cập nhật review
DELETE /api/Review/{id}                    # Xóa review [Admin]
```

## 🔐 Authentication & Authorization

### JWT Token Format

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "Customer",
  "permissions": ["read:hotels", "write:orders"],
  "exp": 1234567890
}
```

### Sử Dụng Token

```bash
curl -H "Authorization: Bearer <your-jwt-token>" \
     http://localhost:5000/api/Hotel
```

### Roles & Permissions

- **Admin**: Full access to all resources
- **Staff**: Order management, view services
- **Customer**: Create orders, reviews, manage own data

## 💾 Database Schema

### Main Tables
- `AspNetUsers` - Người dùng
- `Hotels` - Khách sạn
- `Rooms` - Phòng khách sạn
- `Transports` - Phương tiện vận chuyển
- `TransportTrips` - Chuyến đi
- `Shows` - Show/Sự kiện
- `Orders` - Đơn hàng
- `OrderItems` - Chi tiết đơn hàng
- `Payments` - Thanh toán
- `Reviews` - Đánh giá

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📊 Logging

Logs được lưu trong thư mục `Logs/` với format:
- `log-YYYYMMDD.txt` - Log theo ngày
- Rotating logs tự động

## 🚀 Deployment

### Azure Deployment

```bash
# Build and publish
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deploy --resource-group <rg-name> \
                 --name <app-name> \
                 --src-path ./publish.zip
```

### Docker Deployment

```bash
# Production deployment
./deploy-prod.sh
# hoặc Windows
deploy-prod.bat
```

## 🔒 Security Best Practices

- ✅ JWT tokens với expiration
- ✅ Password hashing với ASP.NET Identity
- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ SQL Injection protection (EF Core)
- ✅ XSS protection
- ✅ Rate limiting (consider implementing)
- ✅ Input validation & sanitization

## 🐛 Common Issues & Solutions

### Database Connection Issues
```bash
# Check connection string
# Verify SQL Server is running
# Check firewall rules for Azure SQL
```

### Redis Connection Issues
```bash
# Verify Redis is running
# Check Redis connection string
# Ensure SSL is configured for Azure Redis
```

## 📝 Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Development/Production | Yes |
| `ConnectionStrings__DefaultConnection` | SQL Server connection | Yes |
| `ConnectionStrings__Redis` | Redis connection | Yes |
| `JwtSettings__SecretKey` | JWT secret key | Yes |
| `GoogleAuth__ClientId` | Google OAuth Client ID | No |

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License

Copyright © 2025 Trippio Team

## 🔗 Related Links

- **Frontend Repository**: [Trippio Frontend](../../../TRIPPIO_FE)
- **Swagger UI**: `http://localhost:5000/swagger` (khi chạy local)
- **Azure Portal**: [Azure Dashboard](https://portal.azure.com)

## 📞 Support & Contact

Nếu bạn gặp vấn đề hoặc có câu hỏi, vui lòng tạo issue trên GitHub repository.

**Author**: Vietokeman  
**GitHub**: https://github.com/Vietokeman/Trippio  
**Facebook**: https://www.facebook.com/vietphomaique123/
