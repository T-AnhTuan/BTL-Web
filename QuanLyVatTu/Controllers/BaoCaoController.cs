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
        [HttpGet]
        public async Task<IActionResult> Kho(int? vatTuId, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                // SỬA LỖI 1: Bắt buộc phải đổ dữ liệu Vật Tư vào ViewBag để View vẽ cái Dropdown
                ViewBag.VatTus = await _context.VatTus.ToListAsync();

                // Giữ lại giá trị ngày tháng người dùng vừa chọn để in lại ra Form
                ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
                ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

                // SỬA LỖI 2: Khởi tạo ĐÚNG đối tượng KhoVM mà file Kho.cshtml đang chờ đợi
                KhoVM model = new KhoVM();
                model.ChiTietGiaoDich = new List<ChiTietGiaoDichViewModel>(); // Khởi tạo list rỗng để không bị Null

                // Nếu người dùng đã chọn 1 vật tư (vatTuId có số)
                if (vatTuId.HasValue && vatTuId.Value > 0)
                {
                    // Lấy thông tin vật tư đó từ Database
                    var vt = await _context.VatTus.FindAsync(vatTuId.Value);
                    if (vt != null)
                    {
                        // Nhét dữ liệu vào KhoVM
                        model.VatTuId = vt.Id;
                        model.TenVatTu = vt.TenVatTu;
                        model.TenKho = "Kho Tổng"; // Tạm fix cứng tên kho
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load danh mục: " + ex.Message);
                return View(new KhoVM());
            }
        }
    }
}