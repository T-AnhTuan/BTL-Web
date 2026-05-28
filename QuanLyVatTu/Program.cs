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
