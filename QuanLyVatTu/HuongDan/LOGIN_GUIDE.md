# Hệ Thống Đăng Nhập và Phân Quyền - Hướng Dẫn Sử Dụng

## 📋 Tổng Quan

Hệ thống quản lý vật tư (QuanLyVatTu) đã được cấu hình với:
- ✅ Đăng nhập và đăng xuất (Login/Logout)
- ✅ Phân quyền dựa trên vai trò (Role-Based Access Control)
- ✅ Bảo mật mật khẩu với BCrypt
- ✅ Kết nối Azure SQL Server
- ✅ Cookie Authentication

## 🔐 Tài Khoản Mặc Định

**Tài khoản Admin:**
- Username: `admin`
- Password: `Admin@123`

## 👥 Các Vai Trò (Roles)

### 1. **Admin** (Quản trị viên)
   - Toàn quyền trong hệ thống
   - Quản lý tài khoản người dùng
   - Quản lý phân quyền
   - Xem báo cáo
   - **Routes được phép:**
	 - `/Home/Index` - Trang chủ
	 - `/Home/Privacy` - Trang riêng tư
	 - `/TaiKhoan/Index` - Danh sách tài khoản
	 - `/TaiKhoan/Create` - Tạo tài khoản
	 - `/TaiKhoan/Edit` - Chỉnh sửa tài khoản
	 - `/TaiKhoan/Delete` - Xóa tài khoản

### 2. **Manager** (Quản lý kho)
   - Quản lý vật tư
   - Quản lý nhập/xuất kho
   - Xem báo cáo
   - **Routes được phép:**
	 - `/Home/Index` - Trang chủ
	 - `/TaiKhoan/MyProfile` - Thông tin cá nhân

### 3. **Staff** (Nhân viên)
   - Xem thông tin vật tư
   - Thực hiện nhập/xuất kho
   - **Routes được phép:**
	 - `/Home/Index` - Trang chủ
	 - `/TaiKhoan/MyProfile` - Thông tin cá nhân

## 🚀 Cách Sử Dụng

### 1. Chạy Migrations để tạo Database

```powershell
# Mở Package Manager Console trong Visual Studio
# Tools > NuGet Package Manager > Package Manager Console

# Tạo migration
Add-Migration InitialCreate

# Cập nhật database
Update-Database
```

### 2. Đăng Nhập

1. Truy cập: `https://localhost:7000/Account/Login` (port có thể khác)
2. Nhập Username: `admin`
3. Nhập Password: `Admin@123`
4. Nhấn "Đăng nhập"

### 3. Tạo Tài Khoản Mới (Chỉ Admin)

1. Đăng nhập với tài khoản Admin
2. Vào: `Quản lý > Tài Khoản`
3. Nhấn "Tạo Tài Khoản"
4. Điền thông tin:
   - Tên đăng nhập (Username)
   - Email
   - Mật khẩu
   - Họ tên
   - Chọn Vai Trò (Role)
5. Nhấn "Lưu"

### 4. Chỉnh Sửa Tài Khoản (Chỉ Admin)

1. Vào: `Quản lý > Tài Khoản`
2. Chọn tài khoản cần chỉnh sửa
3. Nhấn "Chỉnh sửa"
4. Cập nhật thông tin
5. Nhấn "Lưu"

### 5. Xóa Tài Khoản (Chỉ Admin)

1. Vào: `Quản lý > Tài Khoản`
2. Chọn tài khoản cần xóa
3. Nhấn "Xóa"
4. Xác nhận xóa

### 6. Xem Thông Tin Cá Nhân

1. Đăng nhập
2. Vào: `Tài Khoản > Thông Tin Cá Nhân`
3. Xem thông tin đăng nhập gần đây

## 🔧 Cấu Trúc File

```
QuanLyVatTu/
├── Models/
│   ├── User.cs              # Model người dùng
│   └── Role.cs              # Model vai trò
├── ViewModels/
│   └── LoginVm.cs           # View Model đăng nhập
├── Controllers/
│   ├── AccountController.cs # Controller đăng nhập
│   ├── HomeController.cs    # Controller trang chủ
│   └── TaiKhoanController.cs # Controller quản lý tài khoản
├── Services/
│   └── AuthenticationService.cs # Service xác thực
├── Authorization/
│   └── AuthorizeRoleAttribute.cs # Attribute phân quyền
├── Data/
│   └── AppDbContext.cs      # Database context
└── Views/
	├── Account/
	│   ├── Login.cshtml     # Trang đăng nhập
	│   └── AccessDenied.cshtml # Trang truy cập bị từ chối
	└── TaiKhoan/
		├── Index.cshtml     # Danh sách tài khoản
		├── MyProfile.cshtml # Thông tin cá nhân
		├── Create.cshtml    # Tạo tài khoản
		└── Edit.cshtml      # Chỉnh sửa tài khoản
```

## 🔐 Bảo Mật

### Mật Khẩu
- Được mã hóa bằng **BCrypt**
- Không lưu mật khẩu gốc trong database
- Hash được tạo với salt ngẫu nhiên

### Session
- Sử dụng **Cookie Authentication**
- Hết hạn sau 7 ngày (hoặc có thể cấu hình lại)
- Support "Ghi nhớ đăng nhập" (Remember Me)

### Authorization
- Mỗi controller/action có `[Authorize]` attribute
- Các action nhạy cảm có `[Authorize(Roles = "Admin")]`
- Tự động redirect đến login nếu chưa xác thực
- Redirect đến AccessDenied nếu không đủ quyền

## 📱 Quy Trình Đăng Nhập

```
┌─────────────┐
│  User Login │
└──────┬──────┘
	   │
	   ▼
┌─────────────────────────────────┐
│ AccountController.Login (POST)  │
│ - Validate input                │
│ - Authenticate user             │
└──────┬──────────────────────────┘
	   │
	   ▼
┌──────────────────────────┐
│ AuthenticationService    │
│ - Hash password verify   │
│ - Get user từ database   │
└──────┬───────────────────┘
	   │
	   ▼
┌──────────────────────────┐
│ Create Claims            │
│ - NameIdentifier (ID)    │
│ - Name (Username)        │
│ - Email                  │
│ - Role                   │
└──────┬───────────────────┘
	   │
	   ▼
┌──────────────────────────┐
│ SignInAsync              │
│ - Create auth cookie     │
│ - Redirect to home       │
└──────────────────────────┘
```

## 🐛 Troubleshooting

### Lỗi: "Connection string not found"
**Giải pháp:**
- Kiểm tra appsettings.json có connection string không
- Cắp nhật YOUR_SERVER, YOUR_USERNAME, YOUR_PASSWORD

### Lỗi: "Cannot connect to database"
**Giải pháp:**
- Kiểm tra Azure SQL Server firewall rules
- Kiểm tra username/password có chính xác không

### Lỗi: "Đăng nhập thất bại"
**Giải pháp:**
- Kiểm tra username có tồn tại không
- Kiểm tra database đã được create chưa (chạy migrations)

### Lỗi: "Quyền truy cập bị từ chối"
**Giải pháp:**
- Kiểm tra role của user
- Chỉ Admin mới có thể truy cập `/TaiKhoan/Index`

## 📝 Tùy Chỉnh

### Thay Đổi Thời Gian Hết Hạn Session

Sửa trong `Program.cs`:
```csharp
options.ExpireTimeSpan = TimeSpan.FromDays(7); // Thay đổi số ngày
```

### Thêm Vai Trò Mới

Sửa trong `Data/AppDbContext.cs`:
```csharp
modelBuilder.Entity<Role>().HasData(
	new Role { Id = 1, RoleName = "Admin", Description = "..." },
	new Role { Id = 4, RoleName = "NewRole", Description = "..." }
);
```

### Thay Đổi Quyền Truy Cập

Thêm `[Authorize(Roles = "Admin,Manager")]` trên action:
```csharp
[Authorize(Roles = "Admin,Manager")]
public IActionResult ReportPage()
{
	return View();
}
```

## 🔄 Reset Admin Password

Nếu quên mật khẩu Admin, có thể reset trong database:

```sql
-- Lấy hash của mật khẩu mới (dùng BCrypt)
-- BCrypt hash của "Admin@123" là:
-- $2a$11$zx8F9.r5pEDxMDBYk3d5d.fDJ3fDJlZvQJz3fDJ3fDJ3fDJ3fDJ3

UPDATE Users 
SET PasswordHash = '$2a$11$zx8F9.r5pEDxMDBYk3d5d.fDJ3fDJlZvQJz3fDJ3fDJ3fDJ3fDJ3'
WHERE Username = 'admin'
```

## 📞 Hỗ Trợ

Nếu có vấn đề:
1. Kiểm tra lại hướng dẫn trên
2. Xem logs trong Output window
3. Kiểm tra database connection
4. Debug mode: F5 và xem error details
