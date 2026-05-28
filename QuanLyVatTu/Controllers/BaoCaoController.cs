using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    // Chỉ Admin và Quản lý kho mới được xem báo cáo kế toán
    [Authorize(Roles = "Quản trị viên, Quản lý kho")]
    public class BaoCaoController : Controller
    {
        private readonly IBaoCaoService _baoCaoService;
        private readonly AppDbContext _context;

        public BaoCaoController(IBaoCaoService baoCaoService, AppDbContext context)
        {
            _baoCaoService = baoCaoService;
            _context = context;
        }

        // =========================================================================
        // TRANG CHỦ BÁO CÁO: TỔNG HỢP XUẤT - NHẬP - TỒN (Map với Views/BaoCao/Index.cshtml)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Index(int? khoId, DateTime? tuNgay, DateTime? denNgay, string tuKhoa)
        {
            try
            {
                // 1. Chuẩn bị dữ liệu cho Dropdown chọn Kho
                ViewBag.DanhSachKho = new SelectList(await _context.DanhMucKhos.ToListAsync(), "Id", "TenKho", khoId);

                // 2. Thiết lập thời gian mặc định (Từ đầu tháng đến ngày hiện tại)
                if (!tuNgay.HasValue) tuNgay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!denNgay.HasValue) denNgay = DateTime.Now;

                // 3. Gọi Service xử lý thuật toán cộng trừ Tồn đầu - Nhập - Xuất - Tồn cuối
                var viewModel = await _baoCaoService.LayBaoCaoNhapXuatTonAsync(khoId, tuNgay.Value, denNgay.Value, tuKhoa);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi khi tải báo cáo: {ex.Message}");
            }
        }

        // =========================================================================
        // TRANG BIỂU ĐỒ PHÂN TÍCH THỐNG KÊ
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> PhanTichThongKe()
        {
            // Trả về View để Frontend dùng Chart.js vẽ biểu đồ
            return View();
        }
    }
}