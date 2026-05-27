using Microsoft.AspNetCore.Authorization; // Khai báo thư viện để dùng thuộc tính [Authorize] (Yêu cầu đăng nhập)
using Microsoft.AspNetCore.Mvc; // Khai báo thư viện để sử dụng các tính năng của Controller, View
using QuanLyVatTu.Models; // Gọi namespace chứa các lớp Dữ liệu (PhieuNhap, PhieuXuat)
using QuanLyVatTu.Services; // Gọi namespace chứa các Service (INhapXuatService)
using System.Threading.Tasks; // Khai báo thư viện để sử dụng lập trình bất đồng bộ (Task, async, await)
using QuanLyVatTu.ViewModels.BaoCao; // Gọi namespace chứa các ViewModel (frmPhieuNhap, frmPhieuXuat)
namespace QuanLyVatTu.Controllers
{
    // Đánh dấu toàn bộ Controller này yêu cầu người dùng phải đăng nhập mới được truy cập
    [Authorize]
    public class NhapXuatController : Controller
    {
        // -------------------------------------------------------------------------
        // BƯỚC 1: KHAI BÁO BIẾN VÀ TIÊM SERVICE (DEPENDENCY INJECTION)
        // -------------------------------------------------------------------------

        // Khai báo một biến chỉ đọc (_nhapXuatService) mang kiểu dữ liệu là Interface INhapXuatService
        // LƯU Ý: Đã xóa bỏ hoàn toàn AppDbContext và ITinhGiaVonService ở đây
        private readonly INhapXuatService _nhapXuatService;

        // Constructor của Controller: ASP.NET Core sẽ tự động "tiêm" (inject) NhapXuatService vào đây
        public NhapXuatController(INhapXuatService nhapXuatService)
        {
            // Gán service nhận được vào biến cục bộ để các hàm bên dưới sử dụng
            _nhapXuatService = nhapXuatService;
        }

        // -------------------------------------------------------------------------
        // 2. TRANG CHỦ PHÂN HỆ NHẬP XUẤT
        // -------------------------------------------------------------------------

        // Hàm xử lý khi người dùng truy cập đường dẫn /NhapXuat/Index
        public IActionResult Index()
        {
            // Trả về giao diện Index.cshtml (Dashboard của Nhập Xuất)
            return View();
        }

        // =========================================================================
        // 3. NGHIỆP VỤ NHẬP KHO
        // =========================================================================

        // Hàm hiển thị danh sách các Phiếu Nhập (Bất đồng bộ)
        public async Task<IActionResult> DanhSachPhieuNhap()
        {
            // Gọi hàm LayDanhSachPhieuNhapAsync từ Service để lấy dữ liệu thay vì gọi thẳng DbContext
            var danhSach = await _nhapXuatService.LayDanhSachPhieuNhapAsync();

            // Trả danh sách lấy được ra View (DanhSachPhieuNhap.cshtml) để hiển thị lên bảng
            return View(danhSach);
        }

        // Hàm hiển thị Form (Giao diện) để lập phiếu nhập mới (Phương thức GET)
        // Yêu cầu quyền: Chỉ tài khoản có Role là Admin hoặc NhanVienKho mới được vào
        [Authorize(Roles = "Admin, NhanVienKho")]
        public IActionResult LapPhieuNhap()
        {
            // Trả về một Form trống để người dùng nhập liệu
            return View();
        }

        // Hàm xử lý dữ liệu khi người dùng bấm nút "Lưu Phiếu" trên Form (Phương thức POST)
        [HttpPost] // Đánh dấu đây là hàm nhận dữ liệu Post từ Form
        [ValidateAntiForgeryToken] // Bảo mật: Chống tấn công giả mạo request từ trang web khác
        [Authorize(Roles = "Admin, NhanVienKho")] // Kiểm tra quyền truy cập một lần nữa khi submit
        public async Task<IActionResult> LapPhieuNhap(PhieuNhap phieuNhap)
        {
            // Tạo 1 chiếc hộp từ ViewModel
            var formModel = new frmPhieuNhap();

            // Đổ dữ liệu từ Database vào "Phần 3: Dữ liệu mồi" của hộp
            formModel.DanhSachNhaCungCap = await _context.NhaCungCaps.ToListAsync();
            formModel.DanhSachVatTu = await _context.VatTus.ToListAsync();

            // Quăng chiếc hộp đầy đủ đồ nghề này ra cho View (HTML) tự xử lý vẽ dropdown
            return View(formModel);
        }

        // =========================================================================
        // 4. NGHIỆP VỤ XUẤT KHO
        // =========================================================================

        // Hàm hiển thị danh sách các Phiếu Xuất (Bất đồng bộ)
        public async Task<IActionResult> DanhSachPhieuXuat()
        {
            // Ủy quyền cho Service lấy danh sách từ CSDL
            var danhSach = await _nhapXuatService.LayDanhSachPhieuXuatAsync();

            // Đẩy danh sách ra giao diện
            return View(danhSach);
        }

        // Hàm hiển thị Form lập phiếu xuất
        [Authorize(Roles = "Admin, NhanVienKho")]
        public IActionResult LapPhieuXuat()
        {
            // Trả về form trống
            return View();
        }

        // Hàm xử lý lưu phiếu xuất khi bấm Submit Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, NhanVienKho")]
        public async Task<IActionResult> LapPhieuXuat(PhieuXuat phieuXuat)
        {
            // Kiểm tra tính hợp lệ của dữ liệu form
            if (ModelState.IsValid)
            {
                // Gọi Service để thực hiện lưu phiếu xuất xuống Database
                await _nhapXuatService.LapPhieuXuatAsync(phieuXuat);

                // Tạo thông báo thành công
                TempData["SuccessMsg"] = "Đã lập phiếu xuất. Đang chờ Quản lý phê duyệt!";
                // Chuyển hướng về trang danh sách phiếu xuất
                return RedirectToAction(nameof(DanhSachPhieuXuat));
            }
            // Trả lại form nếu lỗi
            return View(phieuXuat);
        }

        // Hàm xử lý việc duyệt phiếu xuất (Duyệt xong mới trừ tồn kho)
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")] // BẢO MẬT: Chỉ có Admin và Manager mới có quyền chạy hàm này
        public async Task<IActionResult> PheDuyetPhieuXuat(int id) // Nhận ID của phiếu xuất cần duyệt từ nút bấm
        {
            // Lấy tên của người quản lý đang đăng nhập thực hiện thao tác duyệt
            string nguoiDuyet = User.Identity.Name;

            // GỌI SERVICE: Đưa ID phiếu và người duyệt cho Service để Service lục Database, trừ Tồn kho
            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, nguoiDuyet);

            // Kiểm tra kết quả trả về từ Service
            if (result.IsSuccess)
            {
                // Nếu đủ hàng và trừ kho thành công
                TempData["SuccessMsg"] = result.Message;
            }
            else
            {
                // Nếu lỗi (ví dụ: Không đủ tồn kho để xuất, phiếu không tồn tại...)
                TempData["ErrorMsg"] = result.Message;
            }

            // Chuyển hướng lại về trang danh sách để load lại trạng thái phiếu
            return RedirectToAction(nameof(DanhSachPhieuXuat));
        }

        // =========================================================================
        // 5. NGHIỆP VỤ KIỂM KÊ KHO
        // =========================================================================

        // Hàm hiển thị danh sách vật tư để nhân viên đi đếm số lượng thực tế
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> KiemKeDinhKy()
        {
            // Gọi Service để lấy danh sách toàn bộ vật tư
            var danhSachTonKho = await _nhapXuatService.LayDanhSachKiemKeAsync();

            // Truyền danh sách ra View để hiển thị
            return View(danhSachTonKho);
        }
    }
}