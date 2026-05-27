using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data; // Nơi chứa WebDbContext
using QuanLyVatTu.Models; // Nơi chứa model NguoiDung, NhatKyHeThong
using QuanLyVatTu.ViewModels.DangNhap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly AppDbContext _context;

        // Tiêm DbContext vào để tương tác với cơ sở dữ liệu
        public NguoiDungController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================================================================
        // 1. QUẢN LÝ ĐĂNG NHẬP / ĐĂNG XUẤT
        // ==============================================================================

        [HttpGet]
        public IActionResult DangNhap(string returnUrl = "/")
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ, không cho vào lại màn hình login
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapVM model, string returnUrl = "/")
        {
            if (ModelState.IsValid)
            {
                // Truy vấn DB kiểm tra tài khoản (Lưu ý: Thực tế cần mã hóa Hash mật khẩu)
                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

                if (user != null)
                {
                    if (user.TrangThaiKhoa)
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!");
                        return View(model);
                    }

                    // 1. Tạo "Thẻ căn cước" (Claims) chứa thông tin người dùng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.NameIdentifier, user.TenDangNhap),
                        new Claim(ClaimTypes.Role, user.QuyenHan) // Quyền: Admin, Manager, NhanVienKho
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = model.NhoMatKhau };

                    // 2. Cấp phát Cookie đăng nhập
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // 3. Ghi log Nhật ký hệ thống
                    await GhiNhatKyAsync(user.TenDangNhap, "Đăng nhập", "Người dùng đăng nhập thành công vào hệ thống.");

                    // 4. Chuyển hướng
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        public async Task<IActionResult> DangXuat()
        {
            // Ghi log trước khi đăng xuất
            await GhiNhatKyAsync(User.Identity.Name, "Đăng xuất", "Người dùng rời khỏi hệ thống.");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap", "NguoiDung");
        }

        // ==============================================================================
        // 2. TỪ CHỐI TRUY CẬP (Access Denied)
        // ==============================================================================

        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            // Trang này hiện ra khi Nhân viên cố tình gõ URL vào trang của Admin
            return View();
        }

        // ==============================================================================
        // 3. QUẢN LÝ TÀI KHOẢN & PHÂN QUYỀN (CHỈ ADMIN MỚI ĐƯỢC VÀO)
        // ==============================================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DanhSachTaiKhoan()
        {
            var users = await _context.NguoiDungs.ToListAsync();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> KhoaMoTaiKhoan(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                // Đảo ngược trạng thái khóa
                user.TrangThaiKhoa = !user.TrangThaiKhoa;
                _context.Update(user);
                await _context.SaveChangesAsync();

                string status = user.TrangThaiKhoa ? "Khóa" : "Mở khóa";
                await GhiNhatKyAsync(User.Identity.Name, "Quản lý tài khoản", $"Đã {status} tài khoản {user.TenDangNhap}");

                TempData["SuccessMsg"] = $"Đã {status} tài khoản thành công!";
            }
            return RedirectToAction(nameof(DanhSachTaiKhoan));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PhanQuyen(int id, string quyenMoi)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                string quyenCu = user.QuyenHan;
                user.QuyenHan = quyenMoi;
                _context.Update(user);
                await _context.SaveChangesAsync();

                await GhiNhatKyAsync(User.Identity.Name, "Phân quyền", $"Thay đổi quyền của {user.TenDangNhap} từ {quyenCu} sang {quyenMoi}");
                TempData["SuccessMsg"] = "Cập nhật quyền thành công!";
            }
            return RedirectToAction(nameof(DanhSachTaiKhoan));
        }

        // ==============================================================================
        // 4. NHẬT KÝ HỆ THỐNG (ADMIN & MANAGER)
        // ==============================================================================

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> LichSuHoatDong()
        {
            // Lấy danh sách lịch sử, sắp xếp mới nhất lên đầu
            var logs = await _context.NhatKyHeThongs
                .OrderByDescending(n => n.ThoiGian)
                .Take(100) // Lấy 100 log gần nhất cho nhẹ web
                .ToListAsync();

            return View(logs);
        }

        // ==============================================================================
        // HÀM HỖ TRỢ: GHI LOG DÙNG CHUNG (KHÔNG PHẢI ACTION)
        // ==============================================================================
        private async Task GhiNhatKyAsync(string nguoiDung, string hanhDong, string chiTiet)
        {
            try
            {
                var log = new NhatKyHeThong
                {
                    TenNguoiDung = nguoiDung ?? "Hệ thống",
                    HanhDong = hanhDong,
                    ChiTiet = chiTiet,
                    ThoiGian = DateTime.Now
                };
                _context.NhatKyHeThongs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Bỏ qua lỗi ghi log để không làm gián đoạn luồng chính của app
                Console.WriteLine("Lỗi ghi log: " + ex.Message);
            }
        }
    }
}