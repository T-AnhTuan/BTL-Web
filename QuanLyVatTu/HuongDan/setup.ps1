#!/usr/bin/env powershell
# QuanLyVatTu - Setup Script
# Hỗ trợ cài đặt Database và Dependencies

param(
	[string]$Command = "help"
)

function Show-Help {
	Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
	Write-Host "║  Quản Lý Vật Tư - Setup Script                            ║" -ForegroundColor Cyan
	Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Lệnh có sẵn:" -ForegroundColor Yellow
	Write-Host "  1. .\setup.ps1 migrate-init        - Tạo migration đầu tiên" -ForegroundColor Green
	Write-Host "  2. .\setup.ps1 migrate-update      - Cập nhật database" -ForegroundColor Green
	Write-Host "  3. .\setup.ps1 migrate-remove      - Xóa migration cuối cùng" -ForegroundColor Green
	Write-Host "  4. .\setup.ps1 db-reset            - Reset database hoàn toàn" -ForegroundColor Green
	Write-Host "  5. .\setup.ps1 db-seed             - Thêm dữ liệu mẫu" -ForegroundColor Green
	Write-Host "  6. .\setup.ps1 install-tools       - Cài đặt dotnet ef tools" -ForegroundColor Green
	Write-Host "  7. .\setup.ps1 run                 - Chạy ứng dụng" -ForegroundColor Green
	Write-Host "  8. .\setup.ps1 build               - Build ứng dụng" -ForegroundColor Green
	Write-Host "  9. .\setup.ps1 help                - Hiển thị trợ giúp này" -ForegroundColor Green
	Write-Host ""
}

function Migrate-Init {
	Write-Host "📦 Tạo migration đầu tiên..." -ForegroundColor Cyan
	dotnet ef migrations add InitialCreate --project QuanLyVatTu
	if ($LASTEXITCODE -eq 0) {
		Write-Host "✅ Migration tạo thành công!" -ForegroundColor Green
	} else {
		Write-Host "❌ Lỗi khi tạo migration" -ForegroundColor Red
	}
}

function Migrate-Update {
	Write-Host "🔄 Cập nhật database..." -ForegroundColor Cyan
	dotnet ef database update --project QuanLyVatTu
	if ($LASTEXITCODE -eq 0) {
		Write-Host "✅ Database cập nhật thành công!" -ForegroundColor Green
		Write-Host ""
		Write-Host "🔑 Thông tin đăng nhập mặc định:" -ForegroundColor Yellow
		Write-Host "   Username: admin" -ForegroundColor White
		Write-Host "   Password: Admin@123" -ForegroundColor White
	} else {
		Write-Host "❌ Lỗi khi cập nhật database" -ForegroundColor Red
	}
}

function Migrate-Remove {
	Write-Host "🗑️  Xóa migration cuối cùng..." -ForegroundColor Cyan
	dotnet ef migrations remove --project QuanLyVatTu
	if ($LASTEXITCODE -eq 0) {
		Write-Host "✅ Migration xóa thành công!" -ForegroundColor Green
	} else {
		Write-Host "❌ Lỗi khi xóa migration" -ForegroundColor Red
	}
}

function Reset-Database {
	Write-Host "⚠️  CẢNH BÁO: Điều này sẽ xóa tất cả dữ liệu!" -ForegroundColor Red
	$confirm = Read-Host "Bạn có chắc chắn? (y/n)"
	if ($confirm -eq "y") {
		Write-Host "🔄 Reset database..." -ForegroundColor Cyan
		dotnet ef database update 0 --project QuanLyVatTu
		dotnet ef database update --project QuanLyVatTu
		Write-Host "✅ Database reset thành công!" -ForegroundColor Green
	} else {
		Write-Host "❌ Hủy bỏ" -ForegroundColor Yellow
	}
}

function Seed-Database {
	Write-Host "🌱 Thêm dữ liệu mẫu..." -ForegroundColor Cyan
	Write-Host "✅ Dữ liệu mẫu đã được thêm trong migration!" -ForegroundColor Green
}

function Install-Tools {
	Write-Host "🛠️  Cài đặt dotnet ef tools..." -ForegroundColor Cyan
	dotnet tool install -g dotnet-ef --version 10.0.0
	Write-Host "✅ Tools cài đặt thành công!" -ForegroundColor Green
}

function Run-App {
	Write-Host "🚀 Chạy ứng dụng..." -ForegroundColor Cyan
	Write-Host "   URL: https://localhost:7XXX" -ForegroundColor Yellow
	dotnet run --project QuanLyVatTu
}

function Build-App {
	Write-Host "🔨 Build ứng dụng..." -ForegroundColor Cyan
	dotnet build QuanLyVatTu
	if ($LASTEXITCODE -eq 0) {
		Write-Host "✅ Build thành công!" -ForegroundColor Green
	} else {
		Write-Host "❌ Build thất bại!" -ForegroundColor Red
	}
}

# Main logic
switch ($Command.ToLower()) {
	"migrate-init" { Migrate-Init }
	"migrate-update" { Migrate-Update }
	"migrate-remove" { Migrate-Remove }
	"db-reset" { Reset-Database }
	"db-seed" { Seed-Database }
	"install-tools" { Install-Tools }
	"run" { Run-App }
	"build" { Build-App }
	"help" { Show-Help }
	default { 
		Write-Host "❌ Lệnh không tìm thấy: $Command" -ForegroundColor Red
		Show-Help
	}
}
