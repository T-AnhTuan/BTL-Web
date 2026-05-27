using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyVatTu.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<INhapXuatService, NhapXuatService>();
builder.Services.AddScoped<ITongHopBaoCaoService, TongHopBaoCaoService>();
builder.Services.AddScoped<ITinhGiaVonService, TinhGiaVonService>();
// Đăng ký dịch vụ xác thực bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/NguoiDung/DangNhap"; // Nơi văng ra khi chưa đăng nhập
        options.AccessDeniedPath = "/NguoiDung/TuChoiTruyCap"; // Nơi văng ra khi SAI QUYỀN
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tự đăng xuất sau 8 tiếng
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/BaoTri");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/BaoTri");
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
