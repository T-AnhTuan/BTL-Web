using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register AppDbContext với connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=tcp:tattech.database.windows.net,1433;Initial Catalog=ExWeb;Persist Security Info=False;User ID=ASPNet_App;Password=Admin@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

builder.Services.AddScoped<INhapXuatService, NhapXuatService>();
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
builder.Services.AddScoped<ITinhGiaVonService, TinhGiaVonService>();

// Đăng ký dịch vụ xác thực bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/NguoiDung/DangNhap"; // Nơi văng ra khi chưa đăng nhập
        options.AccessDeniedPath = "/NguoiDung/DangNhap"; // Nơi văng ra khi SAI QUYỀN
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tự đăng xuất sau 8 tiếng
    });
var app = builder.Build();


// =========================================================================================
// CODE SỬA LỖI: ÉP ĐỒNG BỘ MẬT KHẨU XUỐNG DATABASE AZURE MỖI KHI CHẠY WEB
// =========================================================================================

// Lệnh 1: Tạo ra một không gian dịch vụ (scope) tạm thời vì AppDbContext không thể gọi trực tiếp ở ngoài rìa Program.cs
using (var scope = app.Services.CreateScope())
{
    // Lệnh 2: Kéo công cụ AppDbContext từ trong hệ thống ra để chuẩn bị thao tác với SQL Server
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Lệnh 3: Chạy xuống bảng TaiKhoans, tìm ra tài khoản đầu tiên có tên đăng nhập là "admin"
        var adminUser = context.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == "admin");

        // Lệnh 4: Nếu tìm thấy tài khoản đó (khác null)
        if (adminUser != null)
        {
            // Lệnh 5: Gọi hàm băm của BCrypt, băm lại mật khẩu "Admin@123" chuẩn xác nhất và gán vào DB
            adminUser.MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            // Lệnh 6: Bắt buộc Entity Framework lưu sự thay đổi này xuống Azure ngay lập tức
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Lệnh 7: Bắt lỗi âm thầm nếu DB chưa kịp khởi tạo thì web cũng không bị sập (Crash)
        Console.WriteLine("Lỗi đồng bộ mật khẩu: " + ex.Message);
    }
}
// =========================================================================================


// Cấu hình middleware trong pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/BaoTri");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();