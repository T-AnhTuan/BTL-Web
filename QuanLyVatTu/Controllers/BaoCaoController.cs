using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Services; // Gọi namespace chứa Service bạn vừa tạo
using System;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    // Yêu cầu quyền truy cập: Chỉ Admin và Manager mới được xem Báo cáo
    [Authorize(Roles = "Admin, Manager")]
    public class BaoCaoController : Controller
    {
        // Khai báo giao diện (Interface) của Service thay vì gọi thẳng DbContext
        private readonly ITongHopBaoCaoService _baoCaoService;

        // Tiêm (Inject) Service vào Controller
        public BaoCaoController(ITongHopBaoCaoService baoCaoService)
        {
            _baoCaoService = baoCaoService;
        }

        // =========================================================================
        // 1. TRANG CHỦ BÁO CÁO (Menu chọn loại báo cáo)
        // =========================================================================
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================================
        // 2. BÁO CÁO TỒN KHO
        // =========================================================================
        public async Task<IActionResult> bcTonKho()
        {
            // Gọi Service để lấy dữ liệu đã được tổng hợp sẵn (Dạng BaoCaoTonKhoDto)
            var data = await _baoCaoService.LayDuLieuBaoCaoTonKhoAsync();

            // Truyền gói dữ liệu (data) đó thẳng ra View
            return View(data);
        }

        // =========================================================================
        // 3. BÁO CÁO NHẬP XUẤT (Kèm bộ lọc theo ngày)
        // =========================================================================
        public async Task<IActionResult> bcNhapXuat(DateTime? tuNgay, DateTime? denNgay)
        {
            // Gọi Service và truyền 2 tham số ngày tháng vào
            var data = await _baoCaoService.LayDuLieuBaoCaoNhapXuatAsync(tuNgay, denNgay);

            return View(data);
        }

        // =========================================================================
        // 4. PHÂN TÍCH THỐNG KÊ (Biểu đồ)
        // =========================================================================
        public async Task<IActionResult> PhanTichThongKe()
        {
            // Lấy các mảng dữ liệu (Labels, Data) và tỷ lệ luân chuyển từ Service
            var data = await _baoCaoService.LayDuLieuPhanTichThongKeAsync();

            return View(data);
        }
    }
}