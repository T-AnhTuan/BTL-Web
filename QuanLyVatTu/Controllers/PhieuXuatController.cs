using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    public class PhieuXuatController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly AppDbContext _context;

        public PhieuXuatController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChiTietPhieuXuat(PhieuXuat phieuXuat)
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
