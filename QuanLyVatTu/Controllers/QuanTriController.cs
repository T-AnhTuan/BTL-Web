using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.ViewModels; // Gọi ViewModel
using System.Security.Claims;
using BCrypt.Net;

namespace QuanLyVatTu.Controllers
{
    // Cấp quyền Admin cho toàn bộ Controller này
    [Authorize(Roles = "Admin, Quản trị viên")]
    public class QuanTriController : Controller
    {
        private readonly AppDbContext _context;

        public QuanTriController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // QUẢN LÝ TÀI KHOẢN - GIAO DIỆN
        // ==========================================
        public async Task<IActionResult> QuanLyTaiKhoan()
        {
            ViewBag.VaiTros = await _context.VaiTros
                .AsNoTracking()
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            var taiKhoans = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.NhanVien)
                .Include(t => t.VaiTro)
                .OrderBy(t => t.TenDangNhap)
                .ToListAsync();

            return View(taiKhoans);
        }

        // ==========================================
        // QUẢN LÝ TÀI KHOẢN - CÁC HÀM API (Dành cho Javascript)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetTaiKhoanById(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taiKhoan == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = taiKhoan.Id,
                    tenDangNhap = taiKhoan.TenDangNhap,
                    hoTen = taiKhoan.NhanVien?.HoTen,
                    vaiTroId = taiKhoan.VaiTroId,
                    trangThai = (int)taiKhoan.TrangThai
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> LuuTaiKhoan([FromBody] TaiKhoanCrudDto model)
        {
            try
            {
                if (model.Id == 0) // THÊM MỚI
                {
                    bool isExist = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap);
                    if (isExist) return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

                    var newNhanVien = new NhanVien
                    {
                        MaNV = "NV" + DateTime.Now.Ticks.ToString().Substring(10),
                        HoTen = model.HoTen ?? "Người dùng mới"
                    };
                    _context.NhanViens.Add(newNhanVien);
                    await _context.SaveChangesAsync();

                    var newTaiKhoan = new TaiKhoan
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                        VaiTroId = model.VaiTroId,
                        TrangThai = (TrangThaiTaiKhoan)model.TrangThai,
                        NhanVienId = newNhanVien.Id
                    };
                    _context.TaiKhoans.Add(newTaiKhoan);
                }
                else // CẬP NHẬT
                {
                    var taiKhoan = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(t => t.Id == model.Id);
                    if (taiKhoan == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });

                    // Cập nhật thông tin nhân viên
                    if (taiKhoan.NhanVien != null)
                    {
                        taiKhoan.NhanVien.HoTen = model.HoTen;
                    }

                    // Cập nhật tài khoản
                    taiKhoan.VaiTroId = model.VaiTroId;
                    taiKhoan.TrangThai = (TrangThaiTaiKhoan)model.TrangThai;

                    // Nếu có nhập pass mới thì mới đổi
                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> KhoaMoTaiKhoan(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản" });

            if (tk.TenDangNhap.ToLower() == "admin")
                return Json(new { success = false, message = "Không được phép khóa tài khoản Admin quản trị cao nhất!" });

            tk.TrangThai = tk.TrangThai == TrangThaiTaiKhoan.Active ? TrangThaiTaiKhoan.Locked : TrangThaiTaiKhoan.Active;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            var tk = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(t => t.Id == id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản" });

            if (tk.TenDangNhap.ToLower() == "admin")
                return Json(new { success = false, message = "Không được phép xóa tài khoản Admin quản trị cao nhất!" });

            try
            {
                _context.TaiKhoans.Remove(tk);
                if (tk.NhanVien != null) _context.NhanViens.Remove(tk.NhanVien);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException)
            {
                tk.TrangThai = TrangThaiTaiKhoan.Locked;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = "Tài khoản đã có dữ liệu giao dịch. Đã tự động chuyển sang trạng thái: KHÓA." });
            }
        }

        // ==========================================
        // QUẢN LÝ PHÂN QUYỀN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PhanQuyen(int? vaiTroId)
        {
            var danhSachVaiTro = await _context.VaiTros
                .Include(v => v.PhanQuyens)
                .OrderBy(v => v.Id)
                .ToListAsync();

            // 1. Kiểm tra nếu DB chưa có vai trò nào
            if (danhSachVaiTro == null || !danhSachVaiTro.Any())
                return View(new PhanQuyenVM { DanhSachVaiTro = new List<VaiTro>() });

            // 2. Chọn vai trò: Nếu có ID thì tìm, không có thì lấy vai trò đầu tiên
            var vaiTroDangChon = vaiTroId.HasValue
                ? danhSachVaiTro.FirstOrDefault(v => v.Id == vaiTroId.Value)
                : danhSachVaiTro.FirstOrDefault();

            // 3. Nếu ID truyền vào sai (không tìm thấy), mặc định lấy vai trò đầu tiên
            if (vaiTroDangChon == null) vaiTroDangChon = danhSachVaiTro.First();

            // 4. Logic tự động sinh quyền (Giữ nguyên logic của bạn)
            if (vaiTroDangChon.PhanQuyens == null || !vaiTroDangChon.PhanQuyens.Any())
            {
                var danhSachModule = new List<string> { "Quản lý Danh mục", "Quản lý Vật tư", "Phiếu Nhập", "Phiếu Xuất", "Báo cáo", "Quản trị" };
                foreach (var module in danhSachModule)
                {
                    _context.PhanQuyens.Add(new PhanQuyen { VaiTroId = vaiTroDangChon.Id, TenChucNang = module });
                }
                await _context.SaveChangesAsync();
                vaiTroDangChon = await _context.VaiTros.Include(v => v.PhanQuyens).FirstOrDefaultAsync(v => v.Id == vaiTroDangChon.Id);
            }

            return View(new PhanQuyenVM { DanhSachVaiTro = danhSachVaiTro, VaiTroDangChon = vaiTroDangChon });
        }
        // PHÂN QUYỀN - POST (Lưu dữ liệu cấu hình)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> LuuPhanQuyen(int VaiTroId, List<PhanQuyen> PhanQuyens)
        {
            try
            {
                var vaiTro = await _context.VaiTros
                    .Include(v => v.PhanQuyens)
                    .FirstOrDefaultAsync(v => v.Id == VaiTroId);

                if (vaiTro == null) return NotFound();

                // Cập nhật lại các cờ Checkbox vào Database
                foreach (var quyenMoi in PhanQuyens)
                {
                    var quyenCu = vaiTro.PhanQuyens.FirstOrDefault(q => q.Id == quyenMoi.Id);
                    if (quyenCu != null)
                    {
                        quyenCu.CoQuyenXem = quyenMoi.CoQuyenXem;
                        quyenCu.CoQuyenThem = quyenMoi.CoQuyenThem;
                        quyenCu.CoQuyenSua = quyenMoi.CoQuyenSua;
                        quyenCu.CoQuyenXoa = quyenMoi.CoQuyenXoa;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Lưu cấu hình phân quyền cho [{vaiTro.TenVaiTro}] thành công!";
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu cấu hình: " + ex.Message;
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId });
            }
        }

        // DTO nhận dữ liệu từ JS
        public class TaiKhoanCrudDto
        {
            public int Id { get; set; }
            public string? TenDangNhap { get; set; }
            public string? MatKhau { get; set; }
            public string? HoTen { get; set; }
            public int VaiTroId { get; set; }
            public int TrangThai { get; set; }
        }
    }
}