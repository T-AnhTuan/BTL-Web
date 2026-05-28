using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class NhapXuatController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly AppDbContext _context;

        public NhapXuatController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> PhieuNhap()
        {
            ViewBag.NhaCungCaps = await _context.NhaCungCaps
                .AsNoTracking()
                .Where(n => n.TrangThai == TrangThaiNhaCungCap.Active)
                .OrderBy(n => n.TenNhaCungCap)
                .ToListAsync();
            ViewBag.Khos = await _context.DanhMucKhos
                .AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKho.Active)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            var danhSach = await _context.PhieuNhaps
                .AsNoTracking()
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieuNhap(PhieuNhap phieuNhap)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            phieuNhap.MaPhieu = phieuNhap.MaPhieu?.Trim() ?? string.Empty;
            phieuNhap.GhiChu = phieuNhap.GhiChu?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(phieuNhap.MaPhieu))
            {
                phieuNhap.MaPhieu = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            var validationMessage = await ValidatePhieuNhapAsync(phieuNhap);
            if (validationMessage != null)
            {
                TempData["ErrorMsg"] = validationMessage;
                return RedirectToAction(nameof(PhieuNhap));
            }

            var result = await _nhapXuatService.LapPhieuNhapAsync(phieuNhap, taiKhoanId);

            if (result.IsSuccess)
            {
                TempData["SuccessMsg"] = "Đã tạo thông tin Phiếu Nhập thành công! Vui lòng thêm vật tư.";
            }
            else
            {
                TempData["ErrorMsg"] = result.Message;
            }

            return RedirectToAction(nameof(PhieuNhap));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuNhap(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu nhập.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.TrangThai != TrangThaiPhieuNhap.ChoDuyet)
            {
                TempData["ErrorMsg"] = "Chỉ có thể duyệt phiếu nhập đang chờ duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.ChiTietPhieuNhaps == null || !phieu.ChiTietPhieuNhaps.Any())
            {
                TempData["ErrorMsg"] = "Phiếu nhập chưa có vật tư, không thể duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            phieu.TrangThai = TrangThaiPhieuNhap.DaDuyet;
            await _context.SaveChangesAsync();

            var updatedCost = await _tinhGiaVonService.TinhGiaVonBinhQuanSauKhiNhapAsync(id, taiKhoanId);
            TempData[updatedCost ? "SuccessMsg" : "ErrorMsg"] = updatedCost
                ? "Đã duyệt phiếu nhập và cập nhật tồn kho/giá vốn."
                : "Đã duyệt phiếu nhập nhưng cập nhật tồn kho/giá vốn thất bại.";

            return RedirectToAction(nameof(PhieuNhap));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> HuyPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps.FindAsync(id);
            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu nhập.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.TrangThai != TrangThaiPhieuNhap.ChoDuyet)
            {
                TempData["ErrorMsg"] = "Chỉ có thể hủy phiếu nhập đang chờ duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            phieu.TrangThai = TrangThaiPhieuNhap.TuChoi;
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã hủy phiếu nhập thành công!";

            return RedirectToAction(nameof(PhieuNhap));
        }

        [HttpGet]
        public async Task<IActionResult> PhieuXuat()
        {
            ViewBag.Khos = await _context.DanhMucKhos
                .AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKho.Active)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            var danhSach = await _context.PhieuXuats
                .AsNoTracking()
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats)
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieuXuat(PhieuXuat phieuXuat)
        {
            phieuXuat.MaPhieu = phieuXuat.MaPhieu?.Trim() ?? string.Empty;
            phieuXuat.KhachHang = phieuXuat.KhachHang?.Trim() ?? string.Empty;
            phieuXuat.NguoiXuat = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";
            phieuXuat.LyDoXuat = phieuXuat.LyDoXuat?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(phieuXuat.MaPhieu))
            {
                phieuXuat.MaPhieu = "PX-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            var validationMessage = await ValidatePhieuXuatAsync(phieuXuat);
            if (validationMessage != null)
            {
                TempData["ErrorMsg"] = validationMessage;
                return RedirectToAction(nameof(PhieuXuat));
            }

            phieuXuat.TrangThai = TrangThaiPhieuXuat.ChoDuyet;
            _context.PhieuXuats.Add(phieuXuat);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Tạo thông tin Phiếu Xuất thành công. Hãy thêm vật tư cần xuất!";
            return RedirectToAction(nameof(PhieuXuat));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuXuat(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuXuat));
            }

            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, taiKhoanId);
            if (result.IsSuccess)
            {
                TempData["SuccessMsg"] = result.Message;
            }
            else
            {
                TempData["ErrorMsg"] = result.Message;
            }

            return RedirectToAction(nameof(PhieuXuat));
        }

        private async Task<string?> ValidatePhieuNhapAsync(PhieuNhap phieuNhap)
        {
            if (string.IsNullOrWhiteSpace(phieuNhap.MaPhieu))
            {
                return "Mã phiếu nhập không hợp lệ.";
            }

            if (await _context.PhieuNhaps.AnyAsync(p => p.MaPhieu == phieuNhap.MaPhieu))
            {
                return "Mã phiếu nhập đã tồn tại.";
            }

            if (!await _context.NhaCungCaps.AnyAsync(n => n.Id == phieuNhap.NhaCungCapId && n.TrangThai == TrangThaiNhaCungCap.Active))
            {
                return "Nhà cung cấp không hợp lệ hoặc đã tạm ngừng.";
            }

            if (!await _context.DanhMucKhos.AnyAsync(k => k.Id == phieuNhap.KhoId && k.TrangThai == TrangThaiKho.Active))
            {
                return "Kho nhập không hợp lệ hoặc đã tạm ngừng.";
            }

            return null;
        }

        private async Task<string?> ValidatePhieuXuatAsync(PhieuXuat phieuXuat)
        {
            if (string.IsNullOrWhiteSpace(phieuXuat.MaPhieu))
            {
                return "Mã phiếu xuất không hợp lệ.";
            }

            if (string.IsNullOrWhiteSpace(phieuXuat.KhachHang))
            {
                return "Vui lòng nhập khách hàng/công trình.";
            }

            if (await _context.PhieuXuats.AnyAsync(p => p.MaPhieu == phieuXuat.MaPhieu))
            {
                return "Mã phiếu xuất đã tồn tại.";
            }

            if (!await _context.DanhMucKhos.AnyAsync(k => k.Id == phieuXuat.KhoId && k.TrangThai == TrangThaiKho.Active))
            {
                return "Kho xuất không hợp lệ hoặc đã tạm ngừng.";
            }

            return null;
        }
    }
}
