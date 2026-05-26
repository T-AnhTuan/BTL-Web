# 🎯 Các Lệnh Hữu Ích & Troubleshooting

## ⚡ Lệnh Nhanh

### 1. Chạy Ứng Dụng
```powershell
# Chạy bình thường
dotnet run

# Chạy với hot reload
dotnet watch run

# Chạy với release build
dotnet run -c Release
```

### 2. Build
```powershell
# Build debug
dotnet build

# Build release
dotnet build -c Release

# Build và xuất bản
dotnet publish -c Release -o ./publish
```

### 3. Database Migrations

#### Lần đầu tiên
```powershell
# 1. Tạo migration
Add-Migration InitialCreate

# 2. Update database
Update-Database

# 3. Xem migrations
Get-Migration
```

#### Thêm model mới
```powershell
# 1. Chỉnh sửa model trong Models/
# 2. Tạo migration mới
Add-Migration AddNewFeature

# 3. Kiểm tra migration
# (Sửa nếu cần trong file xxxxxx_AddNewFeature.cs)

# 4. Update database
Update-Database
```

#### Quay lại migration trước
```powershell
# Quay lại migration N bước
Update-Database -Migration:InitialCreate

# Hoặc quay lại hoàn toàn
Update-Database -Migration:0
```

#### Xóa migration
```powershell
# Xóa migration cuối cùng (chưa applied)
Remove-Migration

# Xóa migration cuối cùng (đã applied) - cẩn thận!
Update-Database -Migration:0
Remove-Migration -Force
```

### 4. NuGet Packages

```powershell
# Cài đặt package
Install-Package PackageName -Version 1.0.0

# Cập nhật package
Update-Package PackageName

# Xóa package
Uninstall-Package PackageName

# Liệt kê packages
Get-Package
```

### 5. Entity Framework Core Tools

```powershell
# Cài đặt EF Core tools (global)
dotnet tool install -g dotnet-ef --version 10.0.0

# Cập nhật EF Core tools
dotnet tool update -g dotnet-ef

# Xóa EF Core tools
dotnet tool uninstall -g dotnet-ef

# Kiểm tra version
dotnet ef --version

# Xem context classes
dotnet ef dbcontext list
```

---

## 🔧 Troubleshooting

### ❌ "Connection string not found"

**Nguyên nhân**: Cấu hình không có connection string

**Giải pháp**:
```powershell
# 1. Kiểm tra appsettings.json
Get-Content appsettings.json | ConvertFrom-Json

# 2. Kiểm tra connection string có format đúng không
$conn = "Server=tcp:your.database.windows.net,1433;..."
```

### ❌ "Cannot open database"

**Nguyên nhân**: Database chưa được tạo hoặc credential sai

**Giải pháp**:
```powershell
# 1. Chạy migrations
Update-Database

# 2. Hoặc kiểm tra connection string
# (Ensure server name, database name, username, password correct)

# 3. Test connection
sqlcmd -S your_server.database.windows.net -U username -P password -d QuanLyVatTu -Q "SELECT 1"
```

### ❌ "The specified version cannot be deleted because it is referenced by constraints"

**Nguyên nhân**: Foreign key constraint blocking delete

**Giải pháp**:
```sql
-- Disable constraint checking
ALTER TABLE Users NOCHECK CONSTRAINT ALL

-- Delete data
DELETE FROM Users WHERE Id = 1

-- Enable constraint checking
ALTER TABLE Users CHECK CONSTRAINT ALL
```

### ❌ "Migration hasn't been applied to the database yet"

**Nguyên nhân**: Migration tạo nhưng chưa apply

**Giải pháp**:
```powershell
# Xem pending migrations
Get-Migration -Pending

# Apply migrations
Update-Database
```

### ❌ "Login failed for user"

**Nguyên nhân**: Username/password sai hoặc user không có quyền

**Giải pháp**:
```sql
-- Kiểm tra user tồn tại
SELECT * FROM Users WHERE Username = 'admin'

-- Reset mật khẩu (BCrypt hash của Admin@123)
UPDATE Users 
SET PasswordHash = '$2a$11$DXv0R0JJXSsTtiUQe8KHCeCS0jXGgV3EjuV95UmUiL.kbFFZGGGGa'
WHERE Username = 'admin'
```

### ❌ "Cannot write to disk"

**Nguyên nhân**: Permission issue hoặc disk full

**Giải pháp**:
```powershell
# 1. Kiểm tra disk space
Get-Volume C: | Select-Object SizeRemaining, Size

# 2. Kiểm tra permission
# Chạy Visual Studio as Administrator

# 3. Kiểm tra folder write permission
Test-Path "C:\path\to\project" -PathType Container
```

### ❌ "The project file does not exist"

**Nguyên nhân**: Đường dẫn project sai hoặc file .csproj bị xóa

**Giải pháp**:
```powershell
# 1. Kiểm tra file .csproj tồn tại
Get-Item .\QuanLyVatTu\QuanLyVatTu.csproj

# 2. Kiểm tra directory hiện tại
Get-Location

# 3. Di chuyển đến folder dự án
cd C:\path\to\QuanLyVatTu

# 4. Liệt kê files
ls
```

### ❌ "Unable to find package"

**Nguyên nhân**: Package không tồn tại hoặc version sai

**Giải pháp**:
```powershell
# 1. Kiểm tra package trên NuGet
# https://www.nuget.org

# 2. Cập nhật NuGet package manager
Update-Package -Reinstall

# 3. Xóa bin/obj folder
Remove-Item .\bin -Recurse -Force
Remove-Item .\obj -Recurse -Force

# 4. Restore packages
dotnet restore

# 5. Cài lại package
Install-Package BCrypt.Net-Next -Version 4.0.3
```

### ❌ "The type or namespace name could not be found"

**Nguyên nhân**: Thiếu using statement hoặc reference

**Giải pháp**:
```csharp
// 1. Thêm using statement
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using Microsoft.EntityFrameworkCore;

// 2. Clean & rebuild
// Build > Clean Solution
// Build > Rebuild Solution

// 3. Hoặc từ command line
dotnet clean
dotnet build
```

### ❌ "Could not establish a connection because the server was not found or was not accessible"

**Nguyên nhân**: Firewall, network issue, hoặc server offline

**Giải pháp**:
```powershell
# 1. Ping server
Test-NetConnection your_server.database.windows.net -Port 1433

# 2. Kiểm tra firewall rules
# Azure Portal → SQL Server → Firewall rules
# Thêm current IP

# 3. Kiểm tra network
ipconfig /all

# 4. Test connection
sqlcmd -S your_server.database.windows.net -U username -P password
```

---

## 🐛 Debugging

### Enable EF Core Logging

**Trong Program.cs**:
```csharp
.LogTo(Console.WriteLine, 
	   LogLevel.Information,
	   DbContextLoggerOptions.DefaultWithQueryLogging)
.EnableSensitiveDataLogging()
.EnableDetailedErrors()
```

### Set Breakpoints
```
F9 - Toggle breakpoint
F10 - Step over
F11 - Step into
Ctrl+Shift+F11 - Step out
```

### Watch Window
```
Debug > Windows > Watch
Drag variables vào watch window
```

### Immediate Window
```
Debug > Windows > Immediate (Ctrl+Alt+I)
Chạy code trong debugging session
```

---

## 📊 Performance Testing

### Measure Query Performance

```sql
-- Bật execution time
SET STATISTICS TIME ON

-- Chạy query
SELECT * FROM Users WHERE Username = 'admin'

-- Kết quả
-- SQL Server parse and compile time: ...
```

### Index Usage

```sql
-- Xem indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Users')

-- Xem unused indexes
SELECT * FROM sys.dm_db_index_usage_stats
```

---

## 🔐 Security Testing

### Password Hashing

```csharp
// Test BCrypt
var password = "Admin@123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);
var verified = BCrypt.Net.BCrypt.Verify(password, hash);

Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Verified: {verified}");
```

### SQL Injection Test

```csharp
// Không an toàn (SQL Injection)
var user = $"SELECT * FROM Users WHERE Username = '{username}'";

// An toàn (Parameterized)
var user = context.Users.FromSql($"SELECT * FROM Users WHERE Username = {username}");
```

---

## 📈 Deployment Commands

### Publish to Azure

```powershell
# 1. Build release
dotnet publish -c Release -o ./publish

# 2. Compress
Compress-Archive -Path ./publish -DestinationPath app.zip

# 3. Deploy to Azure App Service
# Az CLI:
az webapp deployment source config-zip --resource-group myGroup --name myApp --src-path app.zip
```

### Database Migration in Production

```powershell
# Backup first!
# Then run migrations with caution
Update-Database -Verbose

# Or use scripts
# dotnet ef migrations script -o migrations.sql
```

---

## 🧹 Cleanup

### Clear Cache

```powershell
# Remove NuGet cache
Remove-Item $PROFILE\.nuget\packages -Recurse -Force

# Clear build artifacts
dotnet clean

# Remove bin/obj
Get-ChildItem -include bin,obj -Recurse | Remove-Item -Recurse -Force
```

### Remove Old Migrations

```powershell
# Xem migrations
Get-Migration

# Remove specific
Remove-Migration -Name OldMigration

# Hoặc xóa tất cả
for ($i=0; $i -lt 10; $i++) { Remove-Migration }
```

---

## 📋 Useful Scripts

### Create Database Backup Script

```powershell
# backup.ps1
param([string]$Database = "QuanLyVatTu")

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "D:\Backups\$Database`_$timestamp.bak"

sqlcmd -S .\SQLEXPRESS -Q "BACKUP DATABASE [$Database] TO DISK = '$backupPath'"
Write-Host "Backup created: $backupPath"
```

### Reset Database Script

```powershell
# reset_db.ps1
Write-Host "Resetting database..." -ForegroundColor Red

# Remove migrations
for ($i=0; $i -lt 10; $i++) { 
	Remove-Migration -Force -ErrorAction SilentlyContinue 
}

# Create new migration
Add-Migration InitialCreate

# Update database
Update-Database

Write-Host "Database reset complete!" -ForegroundColor Green
```

---

## 🔗 Useful URLs

### Local Development
- App: `https://localhost:7XXX`
- Login: `https://localhost:7XXX/Account/Login`
- Swagger (if enabled): `https://localhost:7XXX/swagger`

### Azure
- Portal: `https://portal.azure.com`
- SQL Database: `https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.Sql%2Fservers%2Fdatabases`
- App Service: `https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.Web%2Fsites`

### Tools
- NuGet: `https://www.nuget.org`
- Docs: `https://docs.microsoft.com`
- GitHub: `https://github.com`

---

## 💡 Tips & Tricks

### Tip 1: Use Async/Await
```csharp
// ❌ Blocking
var user = context.Users.FirstOrDefault(u => u.Id == 1);

// ✅ Non-blocking
var user = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
```

### Tip 2: Use LINQ
```csharp
// ❌ In-memory
var users = context.Users.ToList().Where(u => u.RoleId == 1);

// ✅ In database
var users = await context.Users.Where(u => u.RoleId == 1).ToListAsync();
```

### Tip 3: Use Dependency Injection
```csharp
// ❌ Không tốt
var service = new AuthenticationService();

// ✅ Tốt
public class MyController(IAuthenticationService service)
{
}
```

### Tip 4: Use Configuration
```csharp
// ❌ Hardcode
var timeout = 30;

// ✅ From config
var timeout = configuration.GetValue<int>("AppSettings:Timeout");
```

---

## 📞 Quick Reference

| Task | Command |
|------|---------|
| Run app | `dotnet run` |
| Build | `dotnet build` |
| Test | `dotnet test` |
| Create migration | `Add-Migration Name` |
| Update DB | `Update-Database` |
| See migrations | `Get-Migration` |
| Remove migration | `Remove-Migration` |
| Publish | `dotnet publish` |
| Clean | `dotnet clean` |
| Restore | `dotnet restore` |
| New project | `dotnet new webapp` |

---

**Last Updated**: 2024  
**Status**: ✅ Complete
