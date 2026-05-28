using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IBaoCaoService _baoCaoService;

        public HomeController(AppDbContext context, IBaoCaoService baoCaoService)
        {
            _context = context;
            _baoCaoService = baoCaoService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Lấy ID tài khoản đang đăng nhập từ Cookie Claims
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int taiKhoanId = string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);

                // 2. THỐNG KÊ TỔNG QUAN (Dùng đúng trường TonKhoHienTai và GiaVonBinhQuan)
                ViewBag.TongLoaiVatTu = await _context.VatTus.CountAsync();

                var vatTus = await _context.VatTus.ToListAsync();
                // Tính tổng giá trị kho thực tế
                ViewBag.TongGiaTriKho = vatTus.Sum(v => v.TonKhoHienTai * v.GiaVonBinhQuan);

                // 3. CẢNH BÁO TỒN KHO & PHIẾU CHỜ DUYỆT (Dùng Enum TrangThaiPhieu)
                ViewBag.CanhBaoTonKho = await _context.VatTus
                    .CountAsync(v => v.TonKhoHienTai <= v.TonToiThieu);

                ViewBag.PhieuNhapChoPheDuyet = await _context.PhieuNhaps
                    .CountAsync(p => p.TrangThai == TrangThaiPhieuNhap.ChoDuyet);

                ViewBag.PhieuXuatChoPheDuyet = await _context.PhieuXuats
                    .CountAsync(p => p.TrangThai == TrangThaiPhieuXuat.ChoDuyet);

                // 4. LẤY DỮ LIỆU TỪ SERVICE CHO DASHBOARD
                // Danh sách thông báo chưa xem của User này
                ViewBag.ThongBaoMoi = await _baoCaoService.LayThongBaoChuaXemAsync(taiKhoanId);

                // 10 Lịch sử giao dịch/hoạt động mới nhất của toàn hệ thống
                ViewBag.HoatDongGanDay = await _baoCaoService.LayNhatKyGiaoDichMoiNhatAsync(10);

                // Danh sách vật tư đang cạn kiệt cần nhập gấp
                ViewBag.VatTuDuoiMucToiThieu = await _context.VatTus
                    .Where(v => v.TonKhoHienTai < v.TonToiThieu)
                    .OrderBy(v => v.TonKhoHienTai)
                    .Take(10)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                // Bắt lỗi trực tiếp in ra màn hình thay vì văng ra trang Error khó hiểu
                return Content($"LỖI TẠI HOME CONTROLLER: {ex.Message} \nChi tiết: {ex.InnerException?.Message}");
            }
        }

        [HttpGet]
        public IActionResult BaoTri()
        {
            return View();
        }
    }
}