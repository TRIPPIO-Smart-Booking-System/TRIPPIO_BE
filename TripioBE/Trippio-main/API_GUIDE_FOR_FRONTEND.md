# 📚 Trippio API - Hướng Dẫn Cho Frontend

## 🔐 Authentication

### Lấy JWT Token
Sau khi login thành công, backend sẽ trả về JWT token. Lưu token này vào localStorage hoặc sessionStorage.

### Sử dụng Token
Mọi API yêu cầu authentication đều cần gửi token trong header:
```http
Authorization: Bearer <your-jwt-token>
```

**Ví dụ với Axios:**
```javascript
axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
```

**Ví dụ với Fetch:**
```javascript
fetch(url, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
```

---

## 👤 User Profile APIs

### 1. Lấy Thông Tin User Hiện Tại (Chi Tiết)
**Endpoint:** `GET /api/user/me`  
**Auth:** Required ✅  

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+84123456789",
  "isActive": true,
  "avatar": "/media/images/avatar/112024/abc123.jpg",
  "balance": 1000000,
  "lastLoginDate": "2024-11-02T10:30:00Z",
  "dateCreated": "2024-01-01T00:00:00Z",
  "dob": "1990-01-01T00:00:00Z",
  "isEmailVerified": true,
  "isFirstLogin": false,
  "roles": ["Customer", "User"]
}
```

---

### 2. Lấy Thông Tin Profile (Đơn Giản)
**Endpoint:** `GET /api/user/profile`  
**Auth:** Required ✅  

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+84123456789",
  "avatar": "/media/images/avatar/112024/abc123.jpg",
  "balance": 1000000,
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "isEmailVerified": true
}
```

---

## 📸 Upload Avatar - Complete Flow

### Bước 1: Upload File Ảnh
**Endpoint:** `POST /api/media/upload?type=avatar`  
**Auth:** Required ✅  
**Content-Type:** `multipart/form-data`

**Query Parameters:**
- `type` (optional): Loại ảnh - `avatar`, `product`, `banner` (default: `avatar`)

**Request Body (FormData):**
```javascript
const formData = new FormData();
formData.append('file', selectedFile); // File object from <input type="file">
```

**Allowed File Types:**
- `.jpg`, `.jpeg`
- `.png`
- `.gif`
- `.webp`
- `.bmp`

**File Size Limit:** 5MB

**Success Response (200):**
```json
{
  "success": true,
  "path": "/media/images/avatar/112024/abc123.jpg",
  "url": "/media/images/avatar/112024/abc123.jpg",
  "fileName": "abc123.jpg",
  "originalFileName": "my-avatar.jpg",
  "fileSize": 1024000,
  "contentType": "image/jpeg"
}
```

**Error Responses:**
- **400 Bad Request:**
  ```json
  {
    "message": "File type not allowed. Only image files are permitted.",
    "allowedTypes": ".jpg, .jpeg, .png, .gif, .webp, .bmp"
  }
  ```
  hoặc
  ```json
  {
    "message": "File size exceeds 5MB limit"
  }
  ```

- **401 Unauthorized:**
  ```json
  {
    "message": "User not authenticated"
  }
  ```

---

### Bước 2: Cập Nhật Avatar URL
**Endpoint:** `PUT /api/user/avatar`  
**Auth:** Required ✅  
**Content-Type:** `application/json`

**Request Body:**
```json
{
  "avatarUrl": "/media/images/avatar/112024/abc123.jpg"
}
```

**Success Response (200):**
```json
{
  "message": "Avatar updated successfully",
  "avatar": "/media/images/avatar/112024/abc123.jpg"
}
```

**Error Responses:**
- **400 Bad Request:**
  ```json
  {
    "message": "Failed to update avatar",
    "errors": [...]
  }
  ```

- **401 Unauthorized:**
  ```json
  {
    "message": "User ID not found in token."
  }
  ```

- **404 Not Found:**
  ```json
  {
    "message": "User not found."
  }
  ```

---

## 💰 Payment APIs

### Lấy Danh Sách Payments Của User
**Endpoint:** `GET /api/payment/user/{userId}`  
**Auth:** Required ✅  

**Note:** User chỉ có thể xem payments của chính mình. Nếu cố xem của user khác sẽ nhận lỗi 403 Forbidden.

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": [
    {
      "id": "payment-guid",
      "userId": "user-guid",
      "orderId": 123,
      "bookingId": null,
      "amount": 500000,
      "paymentMethod": "PayOS",
      "paidAt": "2024-11-02T10:00:00Z",
      "status": "Paid",
      "paymentLinkId": "pl_123456",
      "orderCode": 1730534400123,
      "order": {
        "id": 123,
        "totalAmount": 500000,
        "orderDate": "2024-11-01T15:30:00Z",
        "status": "Completed",
        "orderItems": [
          {
            "id": 1,
            "price": 300000,
            "quantity": 1,
            "bookingId": "booking-guid",
            "bookingName": "Hotel Booking"
          },
          {
            "id": 2,
            "price": 200000,
            "quantity": 1,
            "bookingId": "booking-guid-2",
            "bookingName": "Transport Booking"
          }
        ]
      },
      "booking": null
    }
  ]
}
```

**Payment Status:**
- `Pending` (0): Đang chờ thanh toán
- `Paid` (1): Đã thanh toán
- `Failed` (2): Thanh toán thất bại
- `Refunded` (3): Đã hoàn tiền

**Order Status:**
- `Pending` (0): Đơn hàng mới
- `Processing` (1): Đang xử lý
- `Completed` (2): Hoàn thành
- `Cancelled` (3): Đã hủy

---

## ⭐ Review APIs

### 1. Tạo Review Cho Order
**Endpoint:** `POST /api/review`  
**Auth:** Required ✅  

**Điều Kiện:**
- Order phải có ít nhất 1 payment với status = `Paid`
- Mỗi customer chỉ review 1 lần cho mỗi order
- Rating: 1-5 (bắt buộc)
- Comment: tối đa 1000 ký tự (optional)

**Request Body:**
```json
{
  "orderId": 123,
  "rating": 5,
  "comment": "Dịch vụ tuyệt vời! Rất hài lòng."
}
```

**Success Response (200):**
```json
{
  "message": "Review created successfully",
  "data": {
    "id": 1,
    "orderId": 123,
    "customerId": "customer-guid",
    "customerName": "John Doe",
    "rating": 5,
    "comment": "Dịch vụ tuyệt vời! Rất hài lòng.",
    "createdAt": "2024-11-02T10:30:00Z"
  }
}
```

**Error Responses:**
- **400 Bad Request:**
  ```json
  {
    "message": "Cannot review this order. Order must have a completed payment and belong to you."
  }
  ```
  hoặc
  ```json
  {
    "message": "You have already reviewed this order."
  }
  ```

---

### 2. Lấy Reviews Của Order
**Endpoint:** `GET /api/review/order/{orderId}`  
**Auth:** Not Required ❌ (Public)

**Response:**
```json
[
  {
    "id": 1,
    "orderId": 123,
    "customerId": "customer-guid",
    "customerName": "John Doe",
    "rating": 5,
    "comment": "Tuyệt vời!",
    "createdAt": "2024-11-02T10:30:00Z"
  },
  {
    "id": 2,
    "orderId": 123,
    "customerId": "customer-guid-2",
    "customerName": "Jane Smith",
    "rating": 4,
    "comment": "Khá tốt",
    "createdAt": "2024-11-02T11:00:00Z"
  }
]
```

---

### 3. Lấy Reviews Của Mình
**Endpoint:** `GET /api/review/my-reviews`  
**Auth:** Required ✅  

**Response:** Giống như endpoint lấy reviews của order

---

### 4. Cập Nhật Review
**Endpoint:** `PUT /api/review/{reviewId}`  
**Auth:** Required ✅  

**Note:** Chỉ customer tạo review mới có thể update

**Request Body:**
```json
{
  "rating": 4,
  "comment": "Cập nhật đánh giá sau khi sử dụng thêm"
}
```

**Success Response (200):**
```json
{
  "message": "Review updated successfully",
  "data": { ... }
}
```

---

### 5. Xóa Review
**Endpoint:** `DELETE /api/review/{reviewId}`  
**Auth:** Required ✅  

**Success Response (200):**
```json
{
  "message": "Review deleted successfully"
}
```

---

### 6. Kiểm Tra Có Thể Review Không
**Endpoint:** `GET /api/review/can-review/{orderId}`  
**Auth:** Required ✅  

**Response:**
```json
{
  "canReview": true
}
```

---

## 📱 Complete Code Examples

### React - Upload Avatar Component

```jsx
import React, { useState } from 'react';
import axios from 'axios';

const AvatarUpload = () => {
  const [uploading, setUploading] = useState(false);
  const [avatarUrl, setAvatarUrl] = useState('');
  const [error, setError] = useState('');

  const API_BASE_URL = 'http://localhost:5000';
  const token = localStorage.getItem('authToken'); // Lấy token từ localStorage

  const handleFileSelect = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Chỉ chấp nhận file ảnh (jpg, png, gif, webp)');
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      setError('File quá lớn. Kích thước tối đa 5MB');
      return;
    }

    setUploading(true);
    setError('');

    try {
      // Step 1: Upload file
      const formData = new FormData();
      formData.append('file', file);

      const uploadResponse = await axios.post(
        `${API_BASE_URL}/api/media/upload?type=avatar`,
        formData,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        }
      );

      const imageUrl = uploadResponse.data.path;

      // Step 2: Update user avatar
      const updateResponse = await axios.put(
        `${API_BASE_URL}/api/user/avatar`,
        { avatarUrl: imageUrl },
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );

      setAvatarUrl(imageUrl);
      alert('Cập nhật avatar thành công!');

    } catch (err) {
      console.error('Upload error:', err);
      setError(err.response?.data?.message || 'Có lỗi xảy ra khi upload ảnh');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="avatar-upload">
      <input
        type="file"
        accept="image/*"
        onChange={handleFileSelect}
        disabled={uploading}
        style={{ display: 'none' }}
        id="avatar-input"
      />
      
      <label htmlFor="avatar-input" style={{ cursor: 'pointer' }}>
        {avatarUrl ? (
          <img 
            src={`${API_BASE_URL}${avatarUrl}`} 
            alt="Avatar" 
            style={{ 
              width: 150, 
              height: 150, 
              borderRadius: '50%',
              objectFit: 'cover'
            }}
          />
        ) : (
          <div style={{ 
            width: 150, 
            height: 150, 
            borderRadius: '50%',
            backgroundColor: '#ddd',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            {uploading ? 'Uploading...' : 'Click to upload'}
          </div>
        )}
      </label>

      {error && <p style={{ color: 'red' }}>{error}</p>}
    </div>
  );
};

export default AvatarUpload;
```

---

### JavaScript (Vanilla) - Upload Avatar

```javascript
// Hàm upload avatar
async function uploadAvatar(fileInput) {
  const file = fileInput.files[0];
  if (!file) return;

  // Validate
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
  if (!allowedTypes.includes(file.type)) {
    alert('Chỉ chấp nhận file ảnh (jpg, png, gif, webp)');
    return;
  }

  if (file.size > 5 * 1024 * 1024) {
    alert('File quá lớn. Kích thước tối đa 5MB');
    return;
  }

  const token = localStorage.getItem('authToken');
  const API_BASE_URL = 'http://localhost:5000';

  try {
    // Step 1: Upload file
    const formData = new FormData();
    formData.append('file', file);

    const uploadResponse = await fetch(
      `${API_BASE_URL}/api/media/upload?type=avatar`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      }
    );

    if (!uploadResponse.ok) {
      throw new Error('Upload failed');
    }

    const uploadData = await uploadResponse.json();
    const imageUrl = uploadData.path;

    // Step 2: Update avatar
    const updateResponse = await fetch(
      `${API_BASE_URL}/api/user/avatar`,
      {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ avatarUrl: imageUrl })
      }
    );

    if (!updateResponse.ok) {
      throw new Error('Update avatar failed');
    }

    // Success! Update UI
    const img = document.getElementById('user-avatar');
    img.src = `${API_BASE_URL}${imageUrl}`;
    
    alert('Cập nhật avatar thành công!');

  } catch (error) {
    console.error('Error:', error);
    alert('Có lỗi xảy ra: ' + error.message);
  }
}

// HTML
// <input type="file" id="avatar-input" accept="image/*" onchange="uploadAvatar(this)">
// <img id="user-avatar" src="" alt="Avatar">
```

---

### React Native - Upload Avatar

```typescript
import React, { useState } from 'react';
import { View, Image, TouchableOpacity, Text, Alert } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import axios from 'axios';

const AvatarUpload = () => {
  const [avatarUrl, setAvatarUrl] = useState('');
  const [uploading, setUploading] = useState(false);

  const API_BASE_URL = 'http://localhost:5000';
  const token = 'your-jwt-token'; // Lấy từ AsyncStorage

  const pickImage = async () => {
    // Request permission
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Cần quyền truy cập thư viện ảnh');
      return;
    }

    // Pick image
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
    });

    if (!result.canceled) {
      uploadImage(result.assets[0]);
    }
  };

  const uploadImage = async (image) => {
    setUploading(true);

    try {
      // Create FormData
      const formData = new FormData();
      formData.append('file', {
        uri: image.uri,
        type: image.type || 'image/jpeg',
        name: image.fileName || 'avatar.jpg',
      });

      // Step 1: Upload
      const uploadResponse = await axios.post(
        `${API_BASE_URL}/api/media/upload?type=avatar`,
        formData,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      const imageUrl = uploadResponse.data.path;

      // Step 2: Update avatar
      await axios.put(
        `${API_BASE_URL}/api/user/avatar`,
        { avatarUrl: imageUrl },
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        }
      );

      setAvatarUrl(imageUrl);
      Alert.alert('Thành công', 'Cập nhật avatar thành công!');

    } catch (error) {
      console.error('Upload error:', error);
      Alert.alert('Lỗi', error.response?.data?.message || 'Có lỗi xảy ra');
    } finally {
      setUploading(false);
    }
  };

  return (
    <View>
      <TouchableOpacity onPress={pickImage} disabled={uploading}>
        {avatarUrl ? (
          <Image 
            source={{ uri: `${API_BASE_URL}${avatarUrl}` }}
            style={{ width: 150, height: 150, borderRadius: 75 }}
          />
        ) : (
          <View style={{ 
            width: 150, 
            height: 150, 
            borderRadius: 75,
            backgroundColor: '#ddd',
            justifyContent: 'center',
            alignItems: 'center'
          }}>
            <Text>{uploading ? 'Uploading...' : 'Tap to upload'}</Text>
          </View>
        )}
      </TouchableOpacity>
    </View>
  );
};

export default AvatarUpload;
```

---

## 🚨 Error Handling Best Practices

### Xử Lý Lỗi Chung
```javascript
async function callAPI(url, options) {
  try {
    const response = await fetch(url, options);
    
    // Check if response is ok
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || `HTTP Error ${response.status}`);
    }
    
    return await response.json();
    
  } catch (error) {
    // Network error hoặc JSON parse error
    if (error instanceof TypeError) {
      console.error('Network error:', error);
      throw new Error('Không thể kết nối đến server');
    }
    
    // API error
    console.error('API error:', error);
    throw error;
  }
}
```

### Xử Lý 401 Unauthorized (Token Expired)
```javascript
// Axios interceptor
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Token expired - redirect to login
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## 📝 Notes Quan Trọng

### 1. **Avatar URL**
- Server trả về **relative path**: `/media/images/avatar/112024/abc123.jpg`
- Frontend cần ghép với base URL để hiển thị:
  ```javascript
  const fullUrl = `${API_BASE_URL}${avatarUrl}`;
  // Result: http://localhost:5000/media/images/avatar/112024/abc123.jpg
  ```

### 2. **File Upload Progress**
Nếu muốn hiển thị progress bar khi upload:
```javascript
axios.post(url, formData, {
  onUploadProgress: (progressEvent) => {
    const percentCompleted = Math.round(
      (progressEvent.loaded * 100) / progressEvent.total
    );
    setUploadProgress(percentCompleted);
  }
});
```

### 3. **Image Preview Trước Khi Upload**
```javascript
function previewImage(file) {
  const reader = new FileReader();
  reader.onload = (e) => {
    document.getElementById('preview').src = e.target.result;
  };
  reader.readAsDataURL(file);
}
```

### 4. **Resize Image Trước Khi Upload** (Để tối ưu bandwidth)
```javascript
// Sử dụng thư viện browser-image-compression
import imageCompression from 'browser-image-compression';

async function compressImage(file) {
  const options = {
    maxSizeMB: 1,
    maxWidthOrHeight: 800,
    useWebWorker: true
  };
  
  try {
    const compressedFile = await imageCompression(file, options);
    return compressedFile;
  } catch (error) {
    console.error('Compression error:', error);
    return file;
  }
}
```

---

## 🔧 Testing với Postman/Thunder Client

### 1. Upload Avatar
```
POST http://localhost:5000/api/media/upload?type=avatar
Headers:
  Authorization: Bearer <your-token>
Body (form-data):
  file: [Select file]
```

### 2. Update Avatar
```
PUT http://localhost:5000/api/user/avatar
Headers:
  Authorization: Bearer <your-token>
  Content-Type: application/json
Body (raw JSON):
{
  "avatarUrl": "/media/images/avatar/112024/abc123.jpg"
}
```

### 3. Get User Info
```
GET http://localhost:5000/api/user/me
Headers:
  Authorization: Bearer <your-token>
```

---

## 📞 Support & Documentation

- **Swagger UI:** http://localhost:5000/swagger
- **API Issues:** Contact backend team
- **Example Project:** [Link to sample frontend project]

---

## 📌 Checklist Cho Frontend Developer

- [ ] Lưu JWT token sau khi login thành công
- [ ] Gửi token trong header `Authorization` cho mọi API cần auth
- [ ] Xử lý lỗi 401 (redirect to login)
- [ ] Validate file type và size trước khi upload
- [ ] Hiển thị loading state khi upload
- [ ] Hiển thị error messages thân thiện với user
- [ ] Ghép base URL với avatar path để hiển thị ảnh
- [ ] Test với file lớn (>5MB) để đảm bảo validation hoạt động
- [ ] Test với file type không hợp lệ
- [ ] Implement image preview trước khi upload (optional)
- [ ] Implement progress bar (optional)

---

**Last Updated:** November 2, 2024  
**Version:** 1.0.0
