using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly AppDbContext _context;

        public NguoiDungController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // ĐĂNG NHẬP
        // ================================================================
        [HttpGet]
        [AllowAnonymous] // Đảm bảo ai cũng vào được trang này
        public IActionResult DangNhap()
        {
            // Nếu đã đăng nhập thì đá thẳng về Home
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(string username, string password)
        {
            // Bọc toàn bộ luồng đăng nhập vào Try-Catch để bắt lỗi hiển thị ra màn hình
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                    return View();
                }

                // 1. Truy vấn bảng TaiKhoans (kèm thông tin Nhân viên và Vai trò)
                var account = await _context.TaiKhoans
                    .Include(t => t.VaiTro)
                    .Include(t => t.NhanVien)
                    .FirstOrDefaultAsync(u => u.TenDangNhap == username);

                if (account == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại trong hệ thống.");
                    return View();
                }

                // 2. Kiểm tra mật khẩu an toàn (Fix lỗi sập BCrypt)
                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.MatKhauHash);
                }
                catch
                {
                    // LƯU Ý: Đoạn Catch này là "phao cứu sinh".
                    // Nếu Database của bạn đang lưu mật khẩu là chữ thường (VD: "Admin@123") thay vì mã Hash
                    // BCrypt sẽ bị lỗi. Đoạn này giúp so sánh chữ thường để bạn vẫn đăng nhập được.
                    if (password == account.MatKhauHash)
                    {
                        isPasswordValid = true;
                    }
                }

                if (!isPasswordValid)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View();
                }

                // 3. Kiểm tra trạng thái khóa
                if (account.TrangThai == TrangThaiTaiKhoan.Locked)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.");
                    return View();
                }

                // 4. Tạo "Thẻ căn cước" (Claims) cho phiên đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.TenDangNhap),
                    new Claim("FullName", account.NhanVien?.HoTen ?? account.TenDangNhap),
                    new Claim(ClaimTypes.Role, account.VaiTro?.TenVaiTro ?? "Khách")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                // Lệnh này sẽ văng lỗi nếu Program.cs chưa cài đặt AddAuthentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 5. Ghi log hệ thống
                await LogActionAsync(account.Id, "Đăng nhập hệ thống thành công");

                // 6. Ép về thẳng Trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Thay vì sập web ra trang Error, nó sẽ in dòng chữ màu đỏ ra ngay trên form Login
                ModelState.AddModelError("", $"LỖI KỸ THUẬT: {ex.Message} (Kiểm tra lại cấu hình Program.cs)");
                return View();
            }
        }

        // ================================================================
        // ĐĂNG XUẤT
        // ================================================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] // Nên có để bảo mật
        public async Task<IActionResult> DangXuat()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                await LogActionAsync(userId, "Đăng xuất khỏi hệ thống");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(DangNhap));
        }

        // ================================================================
        // HÀM HỖ TRỢ GHI LOG (Private)
        // ================================================================
        private async Task LogActionAsync(int taiKhoanId, string action)
        {
            try
            {
                var log = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = action,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };
                _context.NhatKyHeThongs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Bỏ qua nếu lỗi ghi log để không làm gián đoạn luồng chính
            }
        }
    }
}