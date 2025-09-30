Trippio Project - ASP.NET Core & Angular
Giới thiệu

Trippio là một hệ thống đặt vé đa dịch vụ được xây dựng bằng ASP.NET Core cho backend và Angular cho frontend.
Hệ thống hỗ trợ quản lý chỗ ở, vé di chuyển, và vé giải trí, đồng thời cung cấp các tính năng quản trị người dùng, phân quyền, thanh toán và báo cáo.
Mục tiêu của Trippio là mang lại một nền tảng linh hoạt, dễ mở rộng để quản lý và đặt dịch vụ du lịch.

Công nghệ sử dụng

Backend: ASP.NET Core (.NET 8.0), Entity Framework Core, ASP.NET Identity, JWT Authentication, Serilog

Frontend: Angular, CoreUI, TypeScript

Database: SQL Server (hỗ trợ mở rộng PostgreSQL, MySQL)

API: RESTful API, NSwag để generate API Client cho Angular

DevOps: Docker, Docker Compose

Quản lý mã nguồn: GitHub

Tính năng chính
1. Backend (ASP.NET Core)

Xác thực & phân quyền với JWT Token

Quản lý người dùng, vai trò và quyền hạn (Role-based Access Control)

CRUD dịch vụ: chỗ ở, vé di chuyển, vé giải trí

Hỗ trợ phân trang, AutoMapper

Repository + UnitOfWork pattern

Tích hợp Serilog để logging toàn bộ hoạt động

Tích hợp thanh toán, gửi Email & SMS xác nhận đơn hàng

2. Frontend (Angular)

Giao diện đặt vé thân thiện với người dùng

Tìm kiếm & lọc dịch vụ theo danh mục, thời gian, giá

Giỏ hàng & thanh toán

Quản lý tài khoản: đăng ký, đăng nhập, cập nhật hồ sơ

Giao diện Admin: quản lý dịch vụ, người dùng, đơn hàng

Dashboard: thống kê, báo cáo doanh thu

3. DevOps & Triển khai

Cấu hình Docker Compose cho database & service

CI/CD cơ bản với GitHub Actions

Deploy ứng dụng lên server hoặc cloud

Cài đặt & Chạy dự án
Yêu cầu hệ thống

.NET 8.0 SDK

Node.js & Angular CLI

SQL Server / Docker

Visual Studio 2022 / VS Code

Cách chạy dự án
1. Backend
cd backend
# Cài đặt dependencies
dotnet restore
# Chạy migration
dotnet ef database update
# Chạy ứng dụng
dotnet run

2. Frontend
cd frontend
# Cài đặt dependencies
npm install
# Chạy ứng dụng
ng serve --open

Hướng dẫn sử dụng

Đăng ký tài khoản & đăng nhập

Tìm kiếm dịch vụ (chỗ ở, vé di chuyển, vé giải trí)

Thêm dịch vụ vào giỏ hàng & thanh toán

Quản lý đặt vé trong trang cá nhân

Admin quản lý dịch vụ, đơn hàng, người dùng

Liên hệ & Đóng góp

Nếu bạn có bất kỳ đề xuất hoặc muốn đóng góp, vui lòng tạo Pull Request hoặc liên hệ qua GitHub.

Tác giả: Vietokeman

GitHub Repository: https://github.com/Vietokeman/Trippio

Facebook: https://www.facebook.com/vietphomaique123/
