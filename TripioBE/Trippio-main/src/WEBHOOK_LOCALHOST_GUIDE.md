# 🔔 PayOS Webhook - Localhost Testing

## Vấn Đề
PayOS webhook **không thể gọi trực tiếp** localhost vì:
- Localhost không có public IP
- PayOS server ở internet không thể kết nối đến `http://localhost:5000`

## Giải Pháp

### 🎯 Option 1: Test KHÔNG CẦN Webhook (Đơn Giản Nhất)

**Dùng polling để check status:**

```javascript
// Frontend code
async function checkPaymentStatus(orderCode) {
  const response = await fetch(`http://localhost:5000/api/payment/realmoney/${orderCode}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  const data = await response.json();
  
  if (data.status === 'PAID') {
    // Payment successful!
    window.location.href = '/payment-success';
  } else if (data.status === 'CANCELLED') {
    window.location.href = '/payment-cancel';
  }
}

// Poll every 3 seconds
setInterval(() => checkPaymentStatus(123456), 3000);
```

**Ưu điểm:**
- ✅ Không cần setup gì thêm
- ✅ Hoạt động ngay trên localhost
- ✅ Đơn giản, dễ debug

**Nhược điểm:**
- ❌ Không realtime
- ❌ Tốn resources (polling liên tục)

---

### 🚀 Option 2: Dùng ngrok (Khuyến Nghị)

**ngrok tạo tunnel từ internet → localhost**

#### Bước 1: Cài ngrok
```bash
# Download từ https://ngrok.com/download
# Hoặc dùng chocolatey
choco install ngrok
```

#### Bước 2: Chạy API
```bash
cd d:\Ki7fpt\Exe201\TripioBE\Trippio-main\src\Trippio.Api
dotnet run
```

#### Bước 3: Chạy ngrok (terminal mới)
```bash
ngrok http 5000
```

Output:
```
ngrok                                                                    

Session Status                online
Account                       Free (Plan)
Version                       3.0.0
Region                        Asia Pacific (ap)
Forwarding                    https://abc123def456.ngrok.io -> http://localhost:5000

Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00
```

#### Bước 4: Copy HTTPS URL
```
Webhook URL: https://abc123def456.ngrok.io/api/payment/payos-callback
```

#### Bước 5: Cấu Hình PayOS Dashboard
1. Login: https://my.payos.vn
2. Settings → Webhook
3. Nhập: `https://abc123def456.ngrok.io/api/payment/payos-callback`
4. Test webhook (PayOS sẽ gửi test request)
5. Save

#### Bước 6: Test
1. Tạo payment
2. Thanh toán
3. Check logs API:
   ```
   [INFO] Received PayOS webhook for OrderCode: 123456
   [INFO] Payment SUCCESSFUL
   ```

**Ưu điểm:**
- ✅ Realtime notifications
- ✅ Giống production
- ✅ Có thể debug webhook
- ✅ Xem requests trong ngrok UI: http://localhost:4040

**Nhược điểm:**
- ❌ Cần cài thêm tool
- ❌ URL thay đổi mỗi lần chạy (free plan)
- ❌ Phải update webhook URL trong PayOS mỗi lần

---

### 🌍 Option 3: Deploy Lên Server

**Deploy API lên server có public domain**

#### Services Phổ Biến:
- **Azure App Service**: https://azure.microsoft.com (free tier)
- **Railway**: https://railway.app (free tier)
- **Render**: https://render.com (free tier)
- **Heroku**: https://heroku.com

#### After Deploy:
Webhook URL: `https://your-api-domain.azurewebsites.net/api/payment/payos-callback`

**Ưu điểm:**
- ✅ Production-ready
- ✅ URL cố định
- ✅ HTTPS sẵn có
- ✅ Không cần setup gì thêm

**Nhược điểm:**
- ❌ Mất thời gian deploy
- ❌ Khó debug
- ❌ Có thể tốn phí

---

## Recommendation

### Giai Đoạn Development (Đang Làm):
**Option 1** (không webhook) + **Option 2** (ngrok khi cần test webhook)

### Giai Đoạn Testing:
**Option 2** (ngrok) hoặc deploy lên **Railway/Render free tier**

### Giai Đoạn Production:
**Option 3** (Azure/AWS với domain thật)

---

## Test Webhook Locally Với ngrok

### Terminal 1: API
```bash
cd d:\Ki7fpt\Exe201\TripioBE\Trippio-main\src\Trippio.Api
dotnet run
```

### Terminal 2: ngrok
```bash
ngrok http 5000
```

### Terminal 3: Test Payment
```bash
curl -X POST http://localhost:5000/api/payment/realmoney \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "orderCode": 123456,
    "amount": 10000,
    "description": "Test webhook"
  }'
```

### Check Logs
```bash
# API logs
[INFO] Creating PayOS payment for OrderCode: 123456

# Sau khi thanh toán
[INFO] Received PayOS webhook for OrderCode: 123456
[INFO] Payment SUCCESSFUL for OrderCode: 123456, Amount: 10000
```

### Check ngrok UI
```
http://localhost:4040
```
- Xem tất cả requests
- Inspect webhook payload
- Replay requests

---

## FAQ

### Q: Tôi phải dùng webhook không?
**A:** Không bắt buộc cho testing. Dùng polling (check status) cũng được.

### Q: ngrok free có giới hạn gì?
**A:** 
- URL thay đổi mỗi lần chạy
- 40 connections/minute
- 1 ngrok process tại 1 thời điểm

### Q: Làm sao biết webhook đã hoạt động?
**A:** 
- Check logs API
- Check ngrok UI (http://localhost:4040)
- Check PayOS dashboard → Webhook logs

### Q: Webhook URL có thể dùng HTTP không?
**A:** 
- Development: OK (với ngrok HTTPS)
- Production: PHẢI HTTPS

### Q: Tôi không muốn setup ngrok?
**A:** Dùng Option 1 (polling) hoặc deploy lên Railway/Render free tier

---

## Kết Luận

**Cho Testing Nhanh**: Bỏ qua webhook, dùng polling  
**Cho Testing Đầy Đủ**: Dùng ngrok  
**Cho Production**: Deploy lên server với domain thật

**Current Setup**: Bạn đã có API keys, chỉ cần chạy `dotnet run` và test!

🎉 **Không cần webhook vẫn test được payment flow!**
