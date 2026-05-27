using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.ViewModels.DangNhap;
//using QuanLyVatTu.ViewModels.NguoiDung;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    public class NguoiDungController : Controller
    {
        [HttpGet] // Giao diện (GET)
        public IActionResult DangNhap()
        {
            return View();
        }
        // Xử lý khi bấm nút Đăng nhập (POST)
        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapVM model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Kiểm tra tài khoản trong Database (Ở đây tôi làm ví dụ fix cứng)
            // Thực tế bạn sẽ gọi HeThongDbContext để kiểm tra: db.TaiKhoan.FirstOrDefault(...)
            if (model.TenDangNhap == "admin" && model.MatKhau == "123456")
            {
                // 2. Tạo "Thẻ căn cước" (Claims) để lưu thông tin người dùng
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.TenDangNhap),
                    new Claim(ClaimTypes.Role, "Admin"), // Gán quyền Admin
                    new Claim("HoTen", "Nguyễn Văn A")   // Có thể lưu thêm thông tin phụ
                };

                // 3. Đóng dấu xác nhận loại thẻ (Dùng Cookie)
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 4. Lưu thẻ vào trình duyệt của người dùng (Đăng nhập thành công)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // 5. Chuyển hướng về trang chủ
                return RedirectToAction("TongQuan", "TrangChu");
            }

            // Nếu sai mật khẩu
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }
        // Xử lý Đăng xuất
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap", "NguoiDung");
        }
    }
}
