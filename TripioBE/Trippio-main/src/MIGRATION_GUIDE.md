# Database Migration - Add PayOS Fields to Payment Table

## Cần chạy migration để thêm 2 columns mới:
- `PaymentLinkId` (string, nullable)
- `OrderCode` (long, nullable)

## Lệnh tạo migration:

```bash
cd Trippio.Api
dotnet ef migrations add AddPayOSFieldsToPayment --project ../Trippio.Data --startup-project .
```

## Lệnh update database:

```bash
dotnet ef database update --project ../Trippio.Data --startup-project .
```

## Hoặc chạy từ root directory:

```bash
cd src
dotnet ef migrations add AddPayOSFieldsToPayment --project Trippio.Data --startup-project Trippio.Api
dotnet ef database update --project Trippio.Data --startup-project Trippio.Api
```

## SQL Script (nếu muốn chạy thủ công):

```sql
ALTER TABLE [dbo].[Payments]
ADD [PaymentLinkId] NVARCHAR(100) NULL,
    [OrderCode] BIGINT NULL;
```

## Kiểm tra sau khi migration:

```sql
SELECT TOP 1 * FROM Payments;
-- Nên thấy 2 columns mới: PaymentLinkId, OrderCode
```
