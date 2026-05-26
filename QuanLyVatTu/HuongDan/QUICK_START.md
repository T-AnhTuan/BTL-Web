# 🚀 Quick Start - Bắt Đầu Nhanh

## 5 Bước Cơ Bản

### 1️⃣ Cấu Hình Connection String

Mở `appsettings.json` và cập nhật:

```json
{
  "ConnectionStrings": {
	"DatabaseConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=QuanLyVatTu;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
  }
}
```

**Để lấy connection string:**
1. Vào Azure Portal → SQL databases → Database của bạn
2. Chọn **Connection strings**
3. Copy **ADO.NET** connection string
4. Thay `{your_username}` và `{your_password}`

### 2️⃣ Cài Đặt EF Core Tools

Nếu chưa cài, chạy:
```powershell
dotnet tool install -g dotnet-ef --version 10.0.0
```

### 3️⃣ Tạo Database

Mở **Package Manager Console** (Tools > NuGet Package Manager > Package Manager Console)

```powershell
Add-Migration InitialCreate
Update-Database
```

Hoặc sử dụng script:
```powershell
.\setup.ps1 migrate-update
```

### 4️⃣ Chạy Ứng Dụng

```powershell
dotnet run
```

### 5️⃣ Đăng Nhập

- **URL**: https://localhost:7XXX
- **Username**: admin
- **Password**: Admin@123

---

## 🎯 Điều Hành Hàng Ngày

### Xem Danh Sách Tài Khoản
1. Đăng nhập với tài khoản Admin
2. Vào menu: **Quản lý** → **Tài Khoản**

### Tạo Tài Khoản Mới
1. Vào **Quản lý** → **Tài Khoản** → **Tạo Tài Khoản**
2. Điền thông tin
3. Nhấn **Lưu**

### Chỉnh Sửa Tài Khoản
1. Chọn tài khoản cần sửa
2. Nhấn **Chỉnh sửa**
3. Cập nhật thông tin
4. Nhấn **Lưu**

### Xóa Tài Khoản
1. Chọn tài khoản cần xóa
2. Nhấn **Xóa**
3. Xác nhận

---

## ❌ Lỗi Thường Gặp

| Lỗi | Nguyên Nhân | Giải Pháp |
|-----|-----------|----------|
| "Connection string not found" | Chưa cập nhật appsettings.json | Kiểm tra connection string |
| "Cannot connect to server" | Firewall hoặc credentials sai | Thêm firewall rule, kiểm tra user/pass |
| "Migration pending" | Database chưa được update | Chạy `Update-Database` |
| "Login failed" | User không tồn tại hoặc password sai | Chạy migrations, dùng admin/Admin@123 |

---

## 💾 Sao Lưu & Khôi Phục

### Sao Lưu Database

**Sử dụng Azure Portal:**
1. Vào SQL database của bạn
2. **Backups** → **Create backup**

**Sử dụng T-SQL:**
```sql
BACKUP DATABASE [QuanLyVatTu] 
TO DISK = 'D:\backups\QuanLyVatTu_$(Date).bak'
```

### Khôi Phục Database

**Sử dụng Azure Portal:**
1. Vào SQL server
2. **Restore database**
3. Chọn backup point

**Sử dụng T-SQL:**
```sql
RESTORE DATABASE [QuanLyVatTu] 
FROM DISK = 'D:\backups\QuanLyVatTu.bak'
WITH RECOVERY
```

---

## 🔐 Bảo Mật Cơ Bản

### Thay Đổi Mật Khẩu Admin

1. Đăng nhập với tài khoản admin
2. Vào **Quản lý** → **Tài Khoản** → **Chỉnh sửa**
3. Nhập mật khẩu mới
4. Nhấn **Lưu**

### Disable Tài Khoản

1. Vào danh sách tài khoản
2. Uncheck **Hoạt động** (IsActive)
3. Nhấn **Lưu**

### Audit Log

Xem lần đăng nhập gần đây:
- LastLoginDate được cập nhật tự động

---

## 📊 Công Cụ Hữu Ích

### SQL Server Management Studio
```
Server: your_server.database.windows.net
Login: your_username
Password: your_password
```

### Azure Data Studio
- Giao diện hiện đại
- Support T-SQL queries
- Quản lý database trực quan

### Visual Studio Code
```
Extensions cần:
- C# Dev Kit
- SQL Database Projects
```

---

## 🔧 Troubleshooting Nâng Cao

### Kiểm Tra Connection String
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
Get-Content appsettings.Development.json | ConvertFrom-Json
```

### Test Database Connection
```powershell
# PowerShell
$connectionString = "Your connection string"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
Write-Host "Connected successfully"
$connection.Close()
```

### View EF Core Logs
Thêm trong `Program.cs`:
```csharp
.LogTo(Console.WriteLine)
.EnableSensitiveDataLogging()
```

### Reset Migrations Hoàn Toàn
```powershell
# Xóa tất cả migrations
Remove-Migration
# Xóa database
Update-Database -Migration:0
# Tạo lại
Add-Migration InitialCreate
Update-Database
```

---

## 📱 Quy Trình Triển Khai

### Test Locally
```bash
dotnet run
# Kiểm tra tại https://localhost:7XXX
```

### Publish to Azure
```bash
dotnet publish -c Release -o ./publish
# Upload thư mục publish đến Azure App Service
```

### CI/CD Pipeline (Optional)
```yaml
# .github/workflows/deploy.yml
name: Deploy
on: [push]
jobs:
  build:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v2
	  - uses: actions/setup-dotnet@v1
	  - run: dotnet build
	  - run: dotnet publish -c Release
```

---

## 📞 Cần Giúp Đỡ?

1. **Đọc lại hướng dẫn**: [LOGIN_GUIDE.md](LOGIN_GUIDE.md)
2. **Kiểm tra Azure SQL Setup**: [AZURE_SQL_SETUP.md](AZURE_SQL_SETUP.md)
3. **Xem logs**: Output window trong Visual Studio
4. **Google**: Tìm error message

---

## ✅ Checklist Trước Khi Go Live

- [ ] Connection string được cập nhật
- [ ] Database đã được tạo
- [ ] Migrations đã được chạy
- [ ] Có thể đăng nhập với admin account
- [ ] Có thể tạo tài khoản mới
- [ ] HTTPS được bật
- [ ] Firewall rules được cấu hình
- [ ] Backup được tạo
- [ ] Team được huấn luyện

---

**Status**: ✅ Ready to Use  
**Last Updated**: 2024
