using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INhatKyService _nhatKyService;
        public NguoiDungController(AppDbContext context, IWebHostEnvironment webHostEnvironment, INhatKyService nhatKyService)
        {
            _context = context;
            _nhatKyService = nhatKyService;
            _webHostEnvironment = webHostEnvironment;
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
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                    return View();
                }
                var account = await _context.TaiKhoans
                    .Include(t => t.VaiTro)
                    .Include(t => t.NhanVien)
                    .FirstOrDefaultAsync(u => u.TenDangNhap == username);

                if (account == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại trong hệ thống.");
                    return View();
                }
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
              //  await LogActionAsync(account.Id, "Đăng nhập hệ thống thành công");

                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = account.NhanVien?.HoTen != null ? account.NhanVien.Id : account.Id,
                    HanhDong = $"{username} đã đăng nhập hệ thống",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
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
        [HttpGet]
        public async Task<IActionResult> TaiKhoanCaNhan()
        {
            // Lấy ID Tài khoản đang đăng nhập từ Cookie (Claims)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("DangNhap");

            int taiKhoanId = int.Parse(userIdClaim.Value);

            // Truy vấn dữ liệu từ DB, Include bảng NhanVien và VaiTro để lấy đủ thông tin
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .Include(t => t.VaiTro)
                .FirstOrDefaultAsync(t => t.Id == taiKhoanId);

            if (taiKhoan == null || taiKhoan.NhanVien == null) return NotFound();

            // Đổ dữ liệu từ Model (Database) sang ViewModel để truyền ra giao diện
            var viewModel = new TaiKhoanCaNhanVM
            {
                TaiKhoanId = taiKhoan.Id,
                NhanVienId = taiKhoan.NhanVienId,
                HoTen = taiKhoan.NhanVien.HoTen,
                TenDangNhap = taiKhoan.TenDangNhap,
                Email = taiKhoan.NhanVien.Email,
                SoDienThoai = taiKhoan.NhanVien.SoDienThoai,
                VaiTro = taiKhoan.VaiTro.TenVaiTro,
                AvatarUrl = taiKhoan.NhanVien.AvatarUrl
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(TaiKhoanCaNhanVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("TaiKhoanCaNhan", model);
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == model.TaiKhoanId);

            if (taiKhoan == null) return NotFound();

            // 1. CẬP NHẬT THÔNG TIN CƠ BẢN VÀO BẢNG NHÂN VIÊN
            taiKhoan.NhanVien.HoTen = model.HoTen;
            taiKhoan.NhanVien.Email = model.Email;
            taiKhoan.NhanVien.SoDienThoai = model.SoDienThoai;

            // 2. XỬ LÝ UPLOAD ẢNH AVATAR
            if (model.AvatarUpload != null && model.AvatarUpload.Length > 0)
            {
                // Thư mục lưu ảnh: wwwroot/uploads/avatars/
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file ngẫu nhiên để không bị trùng
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarUpload.CopyToAsync(fileStream);
                }

                taiKhoan.NhanVien.AvatarUrl = "/uploads/avatars/" + uniqueFileName;

            }
            // 3. XỬ LÝ ĐỔI MẬT KHẨU (NẾU CÓ NHẬP)
            if (!string.IsNullOrEmpty(model.MatKhauMoi))
            {
                // Kiểm tra xem mật khẩu hiện tại nhập có đúng không
                if (string.IsNullOrEmpty(model.MatKhauHienTai) || !BCrypt.Net.BCrypt.Verify(model.MatKhauHienTai, taiKhoan.MatKhauHash))
                {
                    TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                    return View("TaiKhoanCaNhan", model);
                }

                // Nếu đúng, tiến hành băm (hash) mật khẩu mới và lưu
                taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
            }
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                HanhDong = $"{model.TenDangNhap} đã thay đổi thông tin tài khoản",
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            };
            await _nhatKyService.GhiNhatKyAsync(entry);
            // Lưu tất cả thay đổi xuống Database
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("TaiKhoanCaNhan");
        }

        // ================================================================
        // ĐĂNG XUẤT
        // ================================================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            var tenDangNhap = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                      ?? User.FindFirst("TenDangNhap")?.Value
                      ?? User.Identity?.Name
                      ?? "Unknown";

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {

                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = userId,
                    HanhDong = $"{tenDangNhap} đã đăng xuất khỏi hệ thống",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap", "NguoiDung");
        }

        // ================================================================
        // HÀM HỖ TRỢ GHI LOG (Private)
        // ================================================================
        public class TaiKhoanCrudDto
        {
            public int Id { get; set; }
            public string? TenDangNhap { get; set; }
            public string? MatKhau { get; set; }
            public string? HoTen { get; set; }
            public int VaiTroId { get; set; }
            public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.Active;
        }
    }
}
