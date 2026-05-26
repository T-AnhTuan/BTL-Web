# Hướng dẫn Kết nối Azure SQL Server

## 1. Lấy Connection String từ Azure Portal

### Bước 1: Đăng nhập Azure Portal
- Truy cập https://portal.azure.com
- Đăng nhập bằng tài khoản Azure của bạn

### Bước 2: Tìm SQL Database
- Tìm kiếm "SQL databases" hoặc "SQL servers"
- Chọn SQL Server của bạn

### Bước 3: Lấy Connection String
- Vào **Settings** > **Connection strings**
- Copy connection string từ tab **ADO.NET**
- Connection string sẽ có dạng:
```
Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=QuanLyVatTu;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## 2. Cập nhật Connection String trong ứng dụng

### Tùy chọn A: Cập nhật appsettings.json (Không bảo mật - chỉ để test)
```json
{
  "ConnectionStrings": {
	"DatabaseConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=QuanLyVatTu;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Tùy chọn B: Sử dụng User Secrets (Khuyến cáo cho Development)
```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DatabaseConnection" "Server=tcp:yourserver.database.windows.net,1433;..."
```

### Tùy chọn C: Sử dụng Azure Key Vault (Khuyến cáo cho Production)
```csharp
// Trong Program.cs
var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]);
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

## 3. Chạy Database Migrations

```powershell
# Tạo migration đầu tiên
Add-Migration InitialCreate

# Update database
Update-Database
```

## 4. Xác nhận kết nối thành công

Chạy ứng dụng và kiểm tra:
- Đăng nhập bằng: **admin** / **Admin@123**
- Nếu thành công, bạn sẽ thấy trang Dashboard

## 5. Firewall Rules

Nếu gặp lỗi kết nối, bạn cần thêm IP vào firewall:
- Azure Portal > SQL Server > Firewall rules
- Thêm rule cho IP của máy tính của bạn

## 6. Troubleshooting

### Lỗi: "Cannot open database"
- Kiểm tra tên database có chính xác không
- Kiểm tra credentials

### Lỗi: "Cannot connect to server"
- Kiểm tra server name có đúng không
- Kiểm tra firewall rules

### Lỗi: "Timeout"
- Tăng Connection Timeout trong connection string
- Kiểm tra connection speed

## 7. Độ An Toàn

**Không nên:**
- Commit connection string có password vào Git
- Hardcode credentials trong code

**Nên làm:**
- Sử dụng Azure Key Vault cho production
- Sử dụng User Secrets cho development
- Sử dụng environment variables

## 8. Xóa Database cũ (nếu cần)
```powershell
# Xóa tất cả migrations
Remove-Migration

# Hoặc reset database
Update-Database -Migration:0
```
