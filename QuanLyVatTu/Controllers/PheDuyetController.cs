using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class PheDuyetController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly AppDbContext _context;
        public PheDuyetController(INhapXuatService nhapXuatService, AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _context = context;
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DuyetNhap()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetXuat(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuXuat));
            }

            // GỌI SERVICE: Xử lý nghiệp vụ duyệt, trừ tồn kho và tạo cảnh báo (UPDATE)
            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, taiKhoanId);

            if (result.IsSuccess)
                TempData["SuccessMsg"] = result.Message;
            else
                TempData["ErrorMsg"] = result.Message;

            return RedirectToAction(nameof(PhieuXuat));
        }
    }
}
