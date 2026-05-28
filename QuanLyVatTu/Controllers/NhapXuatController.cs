using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class NhapXuatController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly AppDbContext _context;

        public NhapXuatController(INhapXuatService nhapXuatService, AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _context = context;
        }

        // ================================================================
        // 1. DANH SÁCH PHIẾU XUẤT
        // ================================================================
        public async Task<IActionResult> DanhSachPhieuXuat()
        {
            // Lấy danh sách phiếu xuất đổ ra giao diện
            var danhSach = await _context.PhieuXuats
                .Include(p => p.KhoId)
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();

            return View(danhSach);
        }

        // ================================================================
        // 2. CHỨC NĂNG DUYỆT PHIẾU XUẤT (Chỉ Quản lý mới được phép)
        // ================================================================
        [HttpPost]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuXuat(int id)
        {
            // Lấy trực tiếp ID tài khoản từ Claims (nhanh và an toàn hơn việc Query DB theo Tên)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(DanhSachPhieuXuat));
            }

            // GỌI SERVICE: Xử lý nghiệp vụ duyệt, trừ tồn kho và tạo cảnh báo
            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, taiKhoanId);

            // Bắn thông báo ra giao diện
            if (result.IsSuccess)
            {
                TempData["SuccessMsg"] = result.Message;
            }
            else
            {
                TempData["ErrorMsg"] = result.Message;
            }

            return RedirectToAction(nameof(DanhSachPhieuXuat));
        }

        // ================================================================
        // 3. NGHIỆP VỤ KIỂM KÊ KHO (Chỉ Quản lý mới được phép)
        // ================================================================
        [HttpGet]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> KiemKeDinhKy()
        {
            var danhSachTonKho = await _context.VatTus
                .Include(v => v.DanhMuc)
                .OrderBy(v => v.TenVatTu)
                .ToListAsync();

            return View(danhSachTonKho);
        }
    }
}