# API Usage Examples - PayOS Checkout Integration

## 📋 Mục lục
1. [Thêm sản phẩm vào giỏ hàng](#1-thêm-sản-phẩm-vào-giỏ-hàng)
2. [Xem giỏ hàng](#2-xem-giỏ-hàng)
3. [Bắt đầu checkout](#3-bắt-đầu-checkout)
4. [Kiểm tra trạng thái thanh toán](#4-kiểm-tra-trạng-thái-thanh-toán)
5. [Hủy thanh toán](#5-hủy-thanh-toán)

---

## 1. Thêm sản phẩm vào giỏ hàng

### Request
```http
POST /api/basket/{userId}/items
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "productId": "hotel-deluxe-room-001",
  "productName": "Deluxe Room - 5 Star Hotel",
  "price": 2500000,
  "quantity": 2,
  "productType": "Hotel",
  "imageUrl": "https://example.com/images/deluxe-room.jpg",
  "description": "Luxury room with sea view, breakfast included"
}
```

### Response (200 OK)
```json
{
  "code": 200,
  "message": "Item added to basket successfully",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [
      {
        "productId": "hotel-deluxe-room-001",
        "productName": "Deluxe Room - 5 Star Hotel",
        "price": 2500000,
        "quantity": 2,
        "productType": "Hotel",
        "imageUrl": "https://example.com/images/deluxe-room.jpg",
        "description": "Luxury room with sea view, breakfast included"
      }
    ],
    "total": 5000000,
    "itemCount": 1
  }
}
```

### cURL Example
```bash
curl -X POST "https://yourdomain.com/api/basket/3fa85f64-5717-4562-b3fc-2c963f66afa6/items" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "hotel-deluxe-room-001",
    "productName": "Deluxe Room - 5 Star Hotel",
    "price": 2500000,
    "quantity": 2,
    "productType": "Hotel"
  }'
```

### JavaScript/Fetch Example
```javascript
const userId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
const token = localStorage.getItem('authToken');

const response = await fetch(`/api/basket/${userId}/items`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    productId: 'hotel-deluxe-room-001',
    productName: 'Deluxe Room - 5 Star Hotel',
    price: 2500000,
    quantity: 2,
    productType: 'Hotel'
  })
});

const data = await response.json();
console.log('Basket:', data);
```

---

## 2. Xem giỏ hàng

### Request
```http
GET /api/basket/{userId}
Authorization: Bearer {your_jwt_token}
```

### Response (200 OK)
```json
{
  "code": 200,
  "message": "Basket retrieved successfully",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [
      {
        "productId": "hotel-deluxe-room-001",
        "productName": "Deluxe Room - 5 Star Hotel",
        "price": 2500000,
        "quantity": 2,
        "productType": "Hotel"
      },
      {
        "productId": "tour-halong-bay-002",
        "productName": "Ha Long Bay 2 Days 1 Night Tour",
        "price": 3500000,
        "quantity": 1,
        "productType": "Tour"
      }
    ],
    "total": 8500000,
    "itemCount": 2
  }
}
```

### JavaScript Example
```javascript
const userId = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
const token = localStorage.getItem('authToken');

const response = await fetch(`/api/basket/${userId}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const data = await response.json();
console.log('Current basket:', data);
```

---

## 3. Bắt đầu Checkout

### Request
```http
POST /api/checkout/start
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "buyerName": "Nguyễn Văn A",
  "buyerEmail": "nguyenvana@example.com",
  "buyerPhone": "0912345678"
}
```

**Note:** `userId` có thể bỏ qua vì sẽ lấy từ JWT token tự động.

### Response (200 OK)
```json
{
  "code": 200,
  "message": "Order #123 created successfully. Please complete payment.",
  "data": {
    "checkoutUrl": "https://pay.payos.vn/web/abc123def456",
    "orderCode": 123,
    "amount": 8500000,
    "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
    "paymentLinkId": "pl_abc123def456",
    "status": "PENDING"
  }
}
```

### cURL Example
```bash
curl -X POST "https://yourdomain.com/api/checkout/start" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "buyerName": "Nguyễn Văn A",
    "buyerEmail": "nguyenvana@example.com",
    "buyerPhone": "0912345678"
  }'
```

### JavaScript Example (Full Flow)
```javascript
// Step 1: Start checkout
async function startCheckout() {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch('/api/checkout/start', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      buyerName: 'Nguyễn Văn A',
      buyerEmail: 'nguyenvana@example.com',
      buyerPhone: '0912345678'
    })
  });

  const result = await response.json();
  
  if (result.code === 200) {
    const { checkoutUrl, orderCode, amount, qrCode } = result.data;
    
    console.log('Order created:', orderCode);
    console.log('Amount:', amount, 'VND');
    console.log('Payment URL:', checkoutUrl);
    
    // Option 1: Redirect to PayOS payment page
    window.location.href = checkoutUrl;
    
    // Option 2: Show QR code for mobile payment
    // displayQRCode(qrCode);
    
    // Option 3: Open in new tab
    // window.open(checkoutUrl, '_blank');
  } else {
    alert('Checkout failed: ' + result.message);
  }
}

// Step 2: Display QR code (if using Option 2)
function displayQRCode(qrCodeDataUrl) {
  const qrContainer = document.getElementById('qr-container');
  qrContainer.innerHTML = `
    <div class="qr-payment">
      <h3>Quét mã QR để thanh toán</h3>
      <img src="${qrCodeDataUrl}" alt="QR Code" />
      <p>Hoặc <a href="${checkoutUrl}" target="_blank">thanh toán trực tuyến</a></p>
    </div>
  `;
}
```

### React Example
```jsx
import React, { useState } from 'react';
import axios from 'axios';

function CheckoutButton() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleCheckout = async () => {
    try {
      setLoading(true);
      setError(null);

      const token = localStorage.getItem('authToken');
      
      const response = await axios.post('/api/checkout/start', 
        {
          buyerName: 'Nguyễn Văn A',
          buyerEmail: 'nguyenvana@example.com',
          buyerPhone: '0912345678'
        },
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.data.code === 200) {
        const { checkoutUrl } = response.data.data;
        window.location.href = checkoutUrl; // Redirect to PayOS
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Checkout failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <button onClick={handleCheckout} disabled={loading}>
        {loading ? 'Processing...' : 'Thanh toán ngay'}
      </button>
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </div>
  );
}

export default CheckoutButton;
```

---

## 4. Kiểm tra trạng thái thanh toán

### Request
```http
GET /api/checkout/status/{orderCode}
Authorization: Bearer {your_jwt_token}
```

**Example:**
```http
GET /api/checkout/status/123
Authorization: Bearer {your_jwt_token}
```

### Response (200 OK) - Pending
```json
{
  "code": 200,
  "message": "Payment information retrieved successfully",
  "data": {
    "orderCode": 123,
    "amount": 8500000,
    "status": "PENDING",
    "transactions": []
  }
}
```

### Response (200 OK) - Paid
```json
{
  "code": 200,
  "message": "Payment information retrieved successfully",
  "data": {
    "orderCode": 123,
    "amount": 8500000,
    "status": "PAID",
    "transactions": [
      {
        "reference": "FT23102012345678",
        "amount": 8500000,
        "accountNumber": "19036781588888",
        "description": "MBVCB.1234567890.Payment for Order #123",
        "transactionDateTime": "2025-10-20T14:30:00",
        "virtualAccountName": null,
        "virtualAccountNumber": null,
        "counterAccountBankId": "970422",
        "counterAccountBankName": "Military Commercial Joint Stock Bank",
        "counterAccountName": "NGUYEN VAN A",
        "counterAccountNumber": "0123456789"
      }
    ]
  }
}
```

### JavaScript Example (Polling)
```javascript
// Poll payment status every 3 seconds
async function checkPaymentStatus(orderCode) {
  const token = localStorage.getItem('authToken');
  
  const interval = setInterval(async () => {
    try {
      const response = await fetch(`/api/checkout/status/${orderCode}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      const result = await response.json();
      
      if (result.code === 200) {
        const { status, transactions } = result.data;
        
        console.log('Payment status:', status);
        
        if (status === 'PAID') {
          clearInterval(interval); // Stop polling
          alert('Thanh toán thành công!');
          window.location.href = '/order-success';
        } else if (status === 'CANCELLED') {
          clearInterval(interval);
          alert('Thanh toán đã bị hủy');
          window.location.href = '/payment-cancelled';
        }
      }
    } catch (error) {
      console.error('Error checking status:', error);
    }
  }, 3000); // Check every 3 seconds
  
  // Auto stop after 5 minutes (timeout)
  setTimeout(() => {
    clearInterval(interval);
    console.log('Payment check timeout');
  }, 5 * 60 * 1000);
}

// Start checking after redirect back from PayOS
const urlParams = new URLSearchParams(window.location.search);
const orderCode = urlParams.get('orderCode');
if (orderCode) {
  checkPaymentStatus(orderCode);
}
```

### React Hook Example
```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

function PaymentStatusChecker({ orderCode }) {
  const [status, setStatus] = useState('PENDING');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let interval;
    let timeout;

    const checkStatus = async () => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await axios.get(`/api/checkout/status/${orderCode}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.data.code === 200) {
          const newStatus = response.data.data.status;
          setStatus(newStatus);
          
          if (newStatus === 'PAID' || newStatus === 'CANCELLED') {
            clearInterval(interval);
            clearTimeout(timeout);
            setLoading(false);
          }
        }
      } catch (error) {
        console.error('Error:', error);
      }
    };

    // Initial check
    checkStatus();

    // Poll every 3 seconds
    interval = setInterval(checkStatus, 3000);

    // Timeout after 5 minutes
    timeout = setTimeout(() => {
      clearInterval(interval);
      setLoading(false);
    }, 5 * 60 * 1000);

    return () => {
      clearInterval(interval);
      clearTimeout(timeout);
    };
  }, [orderCode]);

  return (
    <div>
      <h2>Trạng thái thanh toán</h2>
      <p>Order Code: {orderCode}</p>
      <p>Status: <strong>{status}</strong></p>
      {loading && <p>Đang chờ thanh toán...</p>}
      {status === 'PAID' && <p style={{ color: 'green' }}>✅ Thanh toán thành công!</p>}
      {status === 'CANCELLED' && <p style={{ color: 'red' }}>❌ Thanh toán đã bị hủy</p>}
    </div>
  );
}

export default PaymentStatusChecker;
```

---

## 5. Hủy thanh toán

### Request
```http
POST /api/checkout/cancel/{orderCode}
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

"User decided not to purchase"
```

**Example:**
```http
POST /api/checkout/cancel/123
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

"User changed mind"
```

### Response (200 OK)
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

### Response (400 Bad Request) - Already Paid
```json
{
  "code": 400,
  "message": "Failed to cancel checkout: Payment already completed",
  "data": null
}
```

### JavaScript Example
```javascript
async function cancelPayment(orderCode, reason = 'User cancelled') {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch(`/api/checkout/cancel/${orderCode}`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(reason)
  });

  const result = await response.json();
  
  if (result.code === 200) {
    alert('Thanh toán đã được hủy');
    window.location.href = '/basket'; // Redirect back to basket
  } else {
    alert('Không thể hủy: ' + result.message);
  }
}

// Usage
cancelPayment(123, 'User changed mind');
```

---

## 🔄 Complete Frontend Flow Example

### Full Shopping & Checkout Flow
```javascript
class ShoppingCart {
  constructor(apiBaseUrl, authToken) {
    this.apiBaseUrl = apiBaseUrl;
    this.authToken = authToken;
    this.userId = this.getUserIdFromToken();
  }

  // Get userId from JWT token
  getUserIdFromToken() {
    // Decode JWT and extract userId
    const payload = JSON.parse(atob(this.authToken.split('.')[1]));
    return payload.sub || payload.nameid;
  }

  // Add item to basket
  async addToBasket(item) {
    const response = await fetch(`${this.apiBaseUrl}/basket/${this.userId}/items`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.authToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(item)
    });
    return await response.json();
  }

  // Get basket
  async getBasket() {
    const response = await fetch(`${this.apiBaseUrl}/basket/${this.userId}`, {
      headers: {
        'Authorization': `Bearer ${this.authToken}`
      }
    });
    return await response.json();
  }

  // Update item quantity
  async updateQuantity(productId, quantity) {
    const response = await fetch(`${this.apiBaseUrl}/basket/${this.userId}/items/quantity`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${this.authToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ productId, quantity })
    });
    return await response.json();
  }

  // Remove item
  async removeItem(productId) {
    const response = await fetch(`${this.apiBaseUrl}/basket/${this.userId}/items/${productId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${this.authToken}`
      }
    });
    return await response.json();
  }

  // Checkout
  async checkout(buyerInfo) {
    const response = await fetch(`${this.apiBaseUrl}/checkout/start`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.authToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(buyerInfo)
    });
    return await response.json();
  }

  // Check payment status
  async checkPaymentStatus(orderCode) {
    const response = await fetch(`${this.apiBaseUrl}/checkout/status/${orderCode}`, {
      headers: {
        'Authorization': `Bearer ${this.authToken}`
      }
    });
    return await response.json();
  }

  // Cancel payment
  async cancelPayment(orderCode, reason) {
    const response = await fetch(`${this.apiBaseUrl}/checkout/cancel/${orderCode}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.authToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(reason)
    });
    return await response.json();
  }
}

// Usage Example
(async () => {
  const cart = new ShoppingCart('https://yourdomain.com/api', 'your_jwt_token_here');

  // 1. Add items to basket
  await cart.addToBasket({
    productId: 'hotel-001',
    productName: 'Deluxe Room',
    price: 2500000,
    quantity: 2,
    productType: 'Hotel'
  });

  await cart.addToBasket({
    productId: 'tour-002',
    productName: 'Ha Long Bay Tour',
    price: 3500000,
    quantity: 1,
    productType: 'Tour'
  });

  // 2. View basket
  const basket = await cart.getBasket();
  console.log('Basket:', basket);

  // 3. Update quantity
  await cart.updateQuantity('hotel-001', 3);

  // 4. Checkout
  const checkoutResult = await cart.checkout({
    buyerName: 'Nguyễn Văn A',
    buyerEmail: 'nguyenvana@example.com',
    buyerPhone: '0912345678'
  });

  if (checkoutResult.code === 200) {
    const { checkoutUrl, orderCode } = checkoutResult.data;
    
    // Redirect to PayOS
    console.log('Redirecting to:', checkoutUrl);
    // window.location.href = checkoutUrl;
    
    // Or poll for status
    const statusChecker = setInterval(async () => {
      const status = await cart.checkPaymentStatus(orderCode);
      console.log('Status:', status.data.status);
      
      if (status.data.status === 'PAID') {
        clearInterval(statusChecker);
        alert('Payment successful!');
      }
    }, 3000);
  }
})();
```

---

## 📱 Mobile App Integration (React Native)

```javascript
import React, { useState } from 'react';
import { View, Text, Button, Linking } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

const CheckoutScreen = ({ navigation }) => {
  const [loading, setLoading] = useState(false);

  const handleCheckout = async () => {
    try {
      setLoading(true);
      const token = await AsyncStorage.getItem('authToken');
      
      const response = await fetch('https://yourdomain.com/api/checkout/start', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          buyerName: 'Nguyễn Văn A',
          buyerEmail: 'nguyenvana@example.com',
          buyerPhone: '0912345678'
        })
      });

      const result = await response.json();
      
      if (result.code === 200) {
        const { checkoutUrl, orderCode } = result.data;
        
        // Open PayOS in browser
        await Linking.openURL(checkoutUrl);
        
        // Navigate to status checking screen
        navigation.navigate('PaymentStatus', { orderCode });
      }
    } catch (error) {
      console.error('Checkout error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <View>
      <Text>Ready to checkout?</Text>
      <Button 
        title={loading ? 'Processing...' : 'Checkout'} 
        onPress={handleCheckout}
        disabled={loading}
      />
    </View>
  );
};

export default CheckoutScreen;
```

---

## 🎯 Error Handling Examples

### Handle Common Errors
```javascript
async function robustCheckout(buyerInfo) {
  const token = localStorage.getItem('authToken');
  
  try {
    const response = await fetch('/api/checkout/start', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(buyerInfo)
    });

    const result = await response.json();
    
    // Handle different status codes
    switch (result.code) {
      case 200:
        // Success
        window.location.href = result.data.checkoutUrl;
        break;
        
      case 400:
        // Bad request (empty basket, invalid amount, etc.)
        alert(`Lỗi: ${result.message}`);
        break;
        
      case 401:
        // Unauthorized
        alert('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
        window.location.href = '/login';
        break;
        
      case 500:
        // Server error
        alert('Lỗi server. Vui lòng thử lại sau.');
        break;
        
      default:
        alert(`Lỗi không xác định: ${result.message}`);
    }
  } catch (error) {
    console.error('Network error:', error);
    alert('Không thể kết nối server. Kiểm tra kết nối mạng.');
  }
}
```

---

## 📊 Testing with Postman

### Import this collection:

```json
{
  "info": {
    "name": "PayOS Checkout API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Add to Basket",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{authToken}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": "{{baseUrl}}/basket/{{userId}}/items",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"productId\": \"hotel-001\",\n  \"productName\": \"Deluxe Room\",\n  \"price\": 2500000,\n  \"quantity\": 2,\n  \"productType\": \"Hotel\"\n}"
        }
      }
    },
    {
      "name": "Get Basket",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{authToken}}"
          }
        ],
        "url": "{{baseUrl}}/basket/{{userId}}"
      }
    },
    {
      "name": "Start Checkout",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{authToken}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": "{{baseUrl}}/checkout/start",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"buyerName\": \"Nguyen Van A\",\n  \"buyerEmail\": \"test@example.com\",\n  \"buyerPhone\": \"0912345678\"\n}"
        }
      }
    },
    {
      "name": "Check Payment Status",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{authToken}}"
          }
        ],
        "url": "{{baseUrl}}/checkout/status/{{orderCode}}"
      }
    },
    {
      "name": "Cancel Payment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{authToken}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "url": "{{baseUrl}}/checkout/cancel/{{orderCode}}",
        "body": {
          "mode": "raw",
          "raw": "\"User cancelled\""
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://yourdomain.com/api"
    },
    {
      "key": "authToken",
      "value": "your_jwt_token_here"
    },
    {
      "key": "userId",
      "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    {
      "key": "orderCode",
      "value": "123"
    }
  ]
}
```

---

## 🔗 Quick Links

- [Luồng hoạt động chi tiết](./CHECKOUT_PAYOS_FLOW.md)
- [PayOS Documentation](https://payos.vn/docs)
- [PayOS Dashboard](https://my.payos.vn)
