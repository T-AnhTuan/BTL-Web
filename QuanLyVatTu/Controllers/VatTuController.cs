// Khai báo sử dụng các thư viện cần thiết của ASP.NET Core để xác thực, điều hướng và thao tác CSDL
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// Không gian tên của dự án chứa các Controller
namespace QuanLyVatTu.Controllers
{
    // Kế thừa từ lớp Controller cơ sở của MVC để có các tính năng như trả về View()
    public class VatTuController : Controller
    {
        // Khai báo biến _context chỉ đọc để chứa kết nối tới Database SQL Server
        private readonly AppDbContext _context;

        // Hàm khởi tạo (Constructor), tự động được ASP.NET Core bơm (Inject) AppDbContext vào khi chạy
        public VatTuController(AppDbContext context)
        {
            // Gán context nhận được vào biến _context để sử dụng trong toàn bộ Class này
            _context = context;
        }

        // ================================================================
        // 1. TRANG DANH SÁCH VẬT TƯ (Load giao diện và dữ liệu)
        // ================================================================
        // Định nghĩa đây là phương thức xử lý yêu cầu GET từ trình duyệt
        [HttpGet]
        // SỬA LỖI NGHIÊM TRỌNG: Đổi tên hàm từ Index thành dsVatTu để khớp ĐÚNG TÊN với file dsVatTu.cshtml
        public async Task<IActionResult> Index(int? danhMucId, string tuKhoa)
        {
            // Bọc trong khối try-catch để nếu lỗi CSDL thì không bị sập web
            try
            {
                // Truy vấn CSDL: Lấy toàn bộ danh sách DanhMucVatTu và gán vào ViewBag.DanhMucs
                // View dsVatTu.cshtml đang dùng ViewBag.DanhMucs để vẽ thẻ <select> dropdown
                ViewBag.DanhMucs = await _context.DanhMucVatTus.ToListAsync();

                // Bắt đầu viết câu truy vấn lấy Vật tư. Dùng Include để kết nối khóa ngoại lấy Tên Danh Mục (Tránh lỗi Null)
                var query = _context.VatTus.Include(v => v.DanhMuc).AsQueryable();

                // SỬA LỖI: Lấy danh sách chạy thực thi câu lệnh SQL và đổ về RAM dưới dạng List
                var data = await query.ToListAsync();

                // Trả về file dsVatTu.cshtml kèm theo dữ liệu (data) đã truy vấn được
                return View(data);
            }
            catch (Exception ex) // Nếu có lỗi xảy ra
            {
                // In lỗi ra màn hình Console để lập trình viên biết
                Console.WriteLine("Lỗi truy vấn Vật tư: " + ex.Message);
                // Trả về một View trắng hoặc trang báo lỗi tùy chọn
                return View();
            }
        }

        // ================================================================
        // HÀM HỖ TRỢ GHI LOG (Giữ nguyên như code cũ của bạn)
        // ================================================================
        // Khai báo hàm riêng tư (chỉ gọi trong Controller này) để ghi lại lịch sử thao tác
        private async Task LogActionAsync(string action)
        {
            // Lấy ID người dùng đang đăng nhập từ Cookie (dạng chuỗi)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ép kiểu chuỗi ID thành số nguyên (int), nếu thành công thì chạy tiếp
            if (int.TryParse(userIdString, out int taiKhoanId))
            {
                // Tạo một đối tượng Log mới
                var log = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId, // Gắn ID người làm
                    HanhDong = action,       // Gắn hành động (VD: Thêm vật tư, Xóa vật tư)
                    ThoiGian = DateTime.Now, // Gắn thời gian hiện tại
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0" // Gắn IP
                };
                // Thêm log vào hàng đợi của Entity Framework
                _context.NhatKyHeThongs.Add(log);
                // Lưu ý: Không SaveChanges ở đây vì các hàm gọi nó (như Thêm, Xóa) sẽ tự gọi SaveChanges chung
            }
        }
    }
}