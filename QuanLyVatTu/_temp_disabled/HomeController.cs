using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Models; // Nơi chứa các class Models
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    // Bắt buộc đăng nhập mới được vào trang chủ
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; 

        // Tiêm (Inject) cả Logger và WebDbContext vào Controller
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==============================================================================
        // ACTION: INDEX (Đóng vai trò là trang Dashboard hiển thị 4 chức năng chính)
        // ==============================================================================
        public async Task<IActionResult> Index()
        {
            // -------------------------------------------------------------------------
            // 1. HIỂN THỊ TỔNG QUAN (Các con số thống kê lớn trên cùng)
            // -------------------------------------------------------------------------
            // Đếm tổng số loại vật tư đang có trong kho
            ViewBag.TongLoaiVatTu = await _context.VatTus.CountAsync();

            // Tính tổng giá trị kho (Lấy Số lượng * Giá vốn của từng vật tư rồi cộng lại)
            ViewBag.TongGiaTriKho = await _context.VatTus.SumAsync(v => v.SoLuongTon * v.GiaVonTB);

            // Đếm số Phiếu xuất đang "Chờ phê duyệt" để nhắc nhở Manager
            ViewBag.PhieuXuatChoDuyet = await _context.PhieuXuats.CountAsync(p => p.TrangThai == "Chờ phê duyệt");

            // -------------------------------------------------------------------------
            // 2. CẢNH BÁO KHO (Danh sách các vật tư sắp hết hàng)
            // -------------------------------------------------------------------------
            // Giả sử mức tồn tối thiểu quy định chung là 10 (Hoặc có thể lấy từ trường TonToiThieu của bảng Vật Tư)
            var canhBaoKho = await _context.VatTus
                .Where(v => v.SoLuongTon <= 10)
                .OrderBy(v => v.SoLuongTon) // Ưu tiên xếp cái nào ít nhất lên đầu
                .Take(5) // Lấy 5 mặt hàng khẩn cấp nhất để hiện ra trang chủ
                .ToListAsync();

            ViewBag.CanhBaoKho = canhBaoKho;

            // -------------------------------------------------------------------------
            // 3. HOẠT ĐỘNG GẦN ĐÂY (Lấy từ bảng NhatKyHeThong)
            // -------------------------------------------------------------------------
            // Truy vấn 7 hành động mới nhất của các user trong hệ thống
            var hoatDongGanDay = await _context.NhatKyHeThongs
                .OrderByDescending(n => n.ThoiGian)
                .Take(7)
                .ToListAsync();

            ViewBag.HoatDongGanDay = hoatDongGanDay;

            // -------------------------------------------------------------------------
            // 4. XEM THÔNG BÁO (Lấy từ bảng ThongBao - Dành riêng cho User đang đăng nhập)
            // -------------------------------------------------------------------------
            // Chỉ lấy các thông báo chưa đọc, dành cho tất cả mọi người hoặc đúng tên user này
            var thongBaoMoi = await _context.ThongBaos
                .Where(t => t.DaDoc == false && (t.TenNguoiNhan == "All" || t.TenNguoiNhan == User.Identity.Name))
                .OrderByDescending(t => t.NgayTao)
                .Take(5)
                .ToListAsync();

            ViewBag.ThongBaoMoi = thongBaoMoi;

            // Trả toàn bộ các dữ liệu trên về cho View (Index.cshtml) hiển thị
            return View();
        }

        // Khi có lỗi
        [AllowAnonymous]
        public IActionResult BaoTri()
        {
            return View();
        }
    }
}