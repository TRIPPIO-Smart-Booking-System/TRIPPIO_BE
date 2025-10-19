# API Documentation - Forgot Password và Email Verification

## Tổng quan
Đã thêm các endpoint mới cho chức năng quên mật khẩu và cập nhật endpoint login để trả về email khi chưa xác thực.

## Các Model mới

### ForgotPasswordRequest
```csharp
{
    "email": "user@example.com"
}
```

### ResetPasswordRequest
```csharp
{
    "email": "user@example.com",
    "otp": "123456",
    "newPassword": "NewPassword123!"
}
```

### OtpVerificationResult (đã cập nhật)
```csharp
{
    "isSuccess": true/false,
    "message": "...",
    "requireEmailVerification": true/false,
    "requirePhoneVerification": true/false,
    "email": "user@example.com",  // ✅ Trường mới
    "loginResponse": { ... }
}
```

## Endpoints

### 1. POST `/api/admin/auth/login`
**Mô tả**: Đăng nhập (đã cập nhật)

**Request Body**:
```json
{
    "usernameOrPhone": "username hoặc số điện thoại",
    "password": "password"
}
```

**Response** (Khi email chưa verified):
```json
{
    "isSuccess": false,
    "message": "Email verification required for first login. OTP has been sent to your email.",
    "requireEmailVerification": true,
    "requirePhoneVerification": false,
    "email": "user@example.com",  // ✅ Email được trả về
    "loginResponse": null
}
```

**Response** (Khi login thành công):
```json
{
    "isSuccess": true,
    "message": "Login successful",
    "requireEmailVerification": false,
    "requirePhoneVerification": false,
    "email": null,
    "loginResponse": {
        "accessToken": "...",
        "refreshToken": "...",
        "expiresAt": "2025-10-16T12:00:00Z",
        "user": { ... }
    }
}
```

### 2. POST `/api/admin/auth/forgot-password`
**Mô tả**: Yêu cầu đặt lại mật khẩu, gửi OTP về email

**Request Body**:
```json
{
    "email": "user@example.com"
}
```

**Response** (Success - 200 OK):
```json
{
    "message": "If your email exists in our system, you will receive a password reset OTP shortly."
}
```

**Lưu ý**: 
- API luôn trả về message thành công ngay cả khi email không tồn tại (để bảo mật)
- OTP sẽ được gửi qua email và có hiệu lực 10 phút
- Email template sẽ hiển thị mã OTP 6 chữ số

### 3. POST `/api/admin/auth/reset-password`
**Mô tả**: Đặt lại mật khẩu với OTP đã nhận

**Request Body**:
```json
{
    "email": "user@example.com",
    "otp": "123456",
    "newPassword": "NewSecurePassword123!"
}
```

**Response** (Success - 200 OK):
```json
{
    "message": "Password has been reset successfully. You can now login with your new password."
}
```

**Response** (Invalid OTP - 400 Bad Request):
```json
{
    "message": "Invalid or expired OTP"
}
```

**Response** (User not found - 400 Bad Request):
```json
{
    "message": "Invalid request"
}
```

## Flow hoàn chỉnh

### Flow 1: Forgot Password
1. User gọi `/api/admin/auth/forgot-password` với email
2. Server gửi OTP (6 chữ số) về email
3. User nhập OTP và mật khẩu mới
4. User gọi `/api/admin/auth/reset-password` với email, OTP và mật khẩu mới
5. Server xác thực OTP và đặt lại mật khẩu
6. User có thể đăng nhập với mật khẩu mới

### Flow 2: Login khi chưa verify email
1. User gọi `/api/admin/auth/login`
2. Nếu email chưa verified:
   - Server gửi OTP về email
   - Response trả về `requireEmailVerification: true` và `email` của user
3. User nhận OTP từ email
4. User gọi `/api/admin/auth/verify-email` với email và OTP
5. Server verify và tự động đăng nhập user

## Thay đổi Database

### Bảng AppUsers
Đã thêm 2 trường mới:
- `PasswordResetOtp` (nvarchar(max), nullable)
- `PasswordResetOtpExpiry` (datetime2, nullable)

### Migration
Đã tạo migration: `AddPasswordResetOtpFields`

Để áp dụng migration vào database, chạy:
```bash
cd src/Trippio.Data
dotnet ef database update --startup-project ../Trippio.Api/Trippio.Api.csproj
```

## Email Templates

### Email Forgot Password
- **Subject**: "Đặt lại mật khẩu Trippio - Mã OTP"
- **Nội dung**: Hiển thị mã OTP và thời gian hết hạn (10 phút)

### Email Verify Registration
- **Subject**: "Xác thực tài khoản Trippio - Mã OTP"
- **Nội dung**: Hiển thị mã OTP cho việc verify email sau khi đăng ký

## Bảo mật

1. **OTP Expiry**: Tất cả OTP đều hết hạn sau 10 phút
2. **Password Reset Security**: Không tiết lộ thông tin về việc email có tồn tại hay không
3. **Rate Limiting**: Nên thêm rate limiting cho forgot-password endpoint để tránh spam
4. **OTP Validation**: OTP được validate cả về giá trị và thời gian hết hạn

## Testing

### Test Forgot Password Flow
```bash
# 1. Request OTP
curl -X POST http://localhost:5000/api/admin/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'

# 2. Check email và lấy OTP

# 3. Reset password
curl -X POST http://localhost:5000/api/admin/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "otp":"123456",
    "newPassword":"NewPassword123!"
  }'

# 4. Login với password mới
curl -X POST http://localhost:5000/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrPhone":"username",
    "password":"NewPassword123!"
  }'
```

### Test Login Flow (chưa verify)
```bash
# 1. Login với account chưa verify
curl -X POST http://localhost:5000/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrPhone":"username",
    "password":"password"
  }'

# Response sẽ trả về email và requireEmailVerification: true

# 2. Verify email
curl -X POST http://localhost:5000/api/admin/auth/verify-email \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "otp":"123456"
  }'
```
