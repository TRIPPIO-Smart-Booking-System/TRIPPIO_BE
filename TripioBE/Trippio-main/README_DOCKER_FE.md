# HƯỚNG DẪN CHẠY BACKEND TRIPPIO BẰNG DOCKER (CHO FE)

## 1. Chuẩn bị
- Đảm bảo đã cài Docker Desktop
- Đảm bảo đã pull code mới nhất từ repo

## 2. Các bước chạy

**Chạy terminal trong thư mục `/src`**

```sh
cd /src
```

### Bước 1: Pull code mới nhất
```sh
git pull origin main
```

### Bước 2: Kiểm tra volume cũ
```sh
docker volume ls
```

### Bước 3: Xóa volume cache cũ (nếu cần reset data)
```sh
docker volume rm <tên volume>
```

### Bước 4: Build & Run lại backend
```sh
docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build --remove-orphans
```

### Bước 5: Truy cập API
- Mở trình duyệt: http://localhost:5000/swagger (hoặc port bạn config)
- Test API trực tiếp trên Swagger UI

## 3. Lưu ý
- Nếu lỗi volume, xóa volume cũ rồi chạy lại bước 4
- Nếu cần reset data, xóa volume và khởi động lại
- Nếu port bị chiếm, đổi port trong file docker-compose.yml
- Nếu FE cần API docs, xem file `API_GUIDE.md` và `IMPLEMENTATION_SUMMARY.md`

---

**Chúc các bạn FE test vui vẻ!** 🚀