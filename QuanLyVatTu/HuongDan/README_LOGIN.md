# Quản Lý Vật Tư - Hệ Thống Đăng Nhập & Phân Quyền

## ✅ Đã Hoàn Thành

- [x] Hệ thống đăng nhập với Cookie Authentication
- [x] Phân quyền theo vai trò (Role-Based Access Control)
- [x] Quản lý người dùng (Admin chỉ)
- [x] Bảo mật mật khẩu với BCrypt
- [x] Hỗ trợ kết nối Azure SQL Server
- [x] Quản lý session tự động

## 🚀 Các Bước Tiếp Theo

### 1️⃣ **Cấu Hình Azure SQL Server**

Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
	"DatabaseConnection": "Server=tcp:your_server.database.windows.net,1433;Initial Catalog=QuanLyVatTu;Persist Security Info=False;User ID=your_username;Password=your_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

Xem chi tiết: [AZURE_SQL_SETUP.md](AZURE_SQL_SETUP.md)

### 2️⃣ **Tạo Database và Tables**

Mở **Package Manager Console** (Tools > NuGet Package Manager > Package Manager Console):

```powershell
Add-Migration InitialCreate
Update-Database
```

### 3️⃣ **Chạy Ứng Dụng**

```bash
dotnet run
```

Truy cập: `https://localhost:7XXX` (port phụ thuộc vào máy)

### 4️⃣ **Đăng Nhập**

- Username: `admin`
- Password: `Admin@123`

## 📚 Hướng Dẫn Chi Tiết

Xem file: [LOGIN_GUIDE.md](LOGIN_GUIDE.md)

## 📁 Cấu Trúc Thư Mục

```
QuanLyVatTu/
│
├── 📂 Models/
│   ├── User.cs              # Mô hình người dùng
│   └── Role.cs              # Mô hình vai trò
│
├── 📂 ViewModels/
│   └── LoginVm.cs           # ViewModel đăng nhập
│
├── 📂 Controllers/
│   ├── AccountController.cs # Đăng nhập / Đăng xuất
│   ├── HomeController.cs    # Trang chủ
│   └── TaiKhoanController.cs # Quản lý tài khoản
│
├── 📂 Services/
│   └── AuthenticationService.cs # Xác thực & mã hóa
│
├── 📂 Authorization/
│   └── AuthorizeRoleAttribute.cs # Phân quyền
│
├── 📂 Data/
│   └── AppDbContext.cs      # Database context
│
├── 📂 Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── AccessDenied.cshtml
│   └── TaiKhoan/
│       ├── Index.cshtml
│       ├── MyProfile.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
│
├── appsettings.json         # Cấu hình chính
├── appsettings.Development.json # Cấu hình dev
├── Program.cs               # Startup
├── AZURE_SQL_SETUP.md       # Hướng dẫn Azure SQL
└── LOGIN_GUIDE.md           # Hướng dẫn đăng nhập
```

## 🔑 Tính Năng Chính

### 🔐 Đăng Nhập & Bảo Mật
- ✅ Cookie-based authentication
- ✅ Mật khẩu mã hóa BCrypt
- ✅ Session management
- ✅ "Remember me" functionality

### 👥 Quản Lý Người Dùng
- ✅ Tạo tài khoản (Admin only)
- ✅ Chỉnh sửa thông tin
- ✅ Xóa tài khoản
- ✅ Xem lịch sử đăng nhập

### 🛡️ Phân Quyền
- ✅ Admin - Toàn quyền
- ✅ Manager - Quản lý kho
- ✅ Staff - Nhân viên
- ✅ Tự động redirect nếu không đủ quyền

### ☁️ Azure SQL Server
- ✅ Hỗ trợ kết nối đầy đủ
- ✅ Retry policy tự động
- ✅ Connection pooling

## 📊 Roles & Permissions

| Tính Năng | Admin | Manager | Staff |
|-----------|-------|---------|-------|
| Đăng nhập | ✅ | ✅ | ✅ |
| Xem trang chủ | ✅ | ✅ | ✅ |
| Quản lý tài khoản | ✅ | ❌ | ❌ |
| Quản lý vật tư | ✅ | ✅ | ✅ |
| Báo cáo | ✅ | ✅ | ❌ |

## ⚙️ Configuration

### Connection String
- **Production**: Azure SQL Server
- **Development**: Local SQL Server (có thể thay đổi)

### Authentication
- **Type**: Cookie Authentication
- **Timeout**: 7 ngày
- **Scheme**: "CookieAuthentication"

### Password
- **Algorithm**: BCrypt
- **Salt Rounds**: 11 (Default)

## 🔄 Quy Trình Triển Khai

### Development
```bash
# 1. Cập nhật connection string
# 2. Chạy migrations
Add-Migration InitialCreate
Update-Database

# 3. Chạy ứng dụng
dotnet run
```

### Production
```bash
# 1. Sử dụng Azure Key Vault
# 2. Cập nhật firewall rules
# 3. Deploy với HTTPS
dotnet publish -c Release
```

## 🆘 Common Issues & Solutions

### ❌ "Connection string not found"
**Solution**: Cập nhật `appsettings.json` với Azure SQL connection string

### ❌ "Cannot connect to server"
**Solution**: Thêm firewall rule cho IP của bạn trong Azure Portal

### ❌ "Migration pending"
**Solution**: Chạy `Update-Database` trong Package Manager Console

### ❌ "Login failed"
**Solution**: 
- Kiểm tra database đã được tạo chưa
- Kiểm tra user 'admin' có tồn tại không

## 📝 Ghi Chú Quan Trọng

⚠️ **Bảo Mật:**
- Không commit mật khẩu vào Git
- Sử dụng Azure Key Vault cho Production
- Sử dụng User Secrets cho Development

⚠️ **Database:**
- Luôn backup trước khi reset database
- Sử dụng migrations để track thay đổi

⚠️ **Session:**
- Session tự động hết hạn sau 7 ngày
- User cần đăng nhập lại sau logout

## 📞 Liên Hệ & Hỗ Trợ

Nếu có vấn đề:
1. Kiểm tra logs (Output window)
2. Xem error message chi tiết
3. Kiểm tra database connection
4. Đọc lại [LOGIN_GUIDE.md](LOGIN_GUIDE.md)

## ✨ Tính Năng Sắp Tới

- [ ] Two-Factor Authentication (2FA)
- [ ] OAuth2 / OpenID Connect
- [ ] Audit logging
- [ ] Password reset by email
- [ ] User profile management
- [ ] Activity tracking

---

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: ✅ Ready to Use
