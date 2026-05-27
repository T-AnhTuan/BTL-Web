using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services; // Thư mục chứa INhapXuatService

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class NhapXuatController : Controller
    {
        // Khai báo Interface của Service thay vì gọi AppDbContext
        private readonly INhapXuatService _nhapXuatService;

        // Tiêm (Inject) NhapXuatService vào Controller
        public NhapXuatController(INhapXuatService nhapXuatService)
        {
            _nhapXuatService = nhapXuatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // =========================================================================
        // NGHIỆP VỤ NHẬP KHO
        // =========================================================================

        public async Task<IActionResult> DanhSachPhieuNhap()
        {
            // Gọi Service để lấy dữ liệu
            var danhSach = await _nhapXuatService.LayDanhSachPhieuNhapAsync();
            return View(danhSach);
        }

        [Authorize(Roles = "Admin, NhanVienKho")]
        public IActionResult LapPhieuNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, NhanVienKho")]
        public async Task<IActionResult> LapPhieuNhap(PhieuNhap phieuNhap)
        {
            // Validation dữ liệu form vẫn nên để ở Controller
            if (ModelState.IsValid)
            {
                // Lấy tên user đăng nhập để truyền xuống Service
                string nguoiLap = User.Identity.Name;

                // Gọi Service xử lý lưu và tính giá vốn
                var result = await _nhapXuatService.LapPhieuNhapAsync(phieuNhap, nguoiLap);

                if (result.IsSuccess)
                {
                    TempData["SuccessMsg"] = result.Message;
                    return RedirectToAction(nameof(DanhSachPhieuNhap));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }

            // Trả về view nếu nhập thiếu thông tin
            return View(phieuNhap);
        }

        // =========================================================================
        // NGHIỆP VỤ XUẤT KHO
        // =========================================================================

        public async Task<IActionResult> DanhSachPhieuXuat()
        {
            var danhSach = await _nhapXuatService.LayDanhSachPhieuXuatAsync();
            return View(danhSach);
        }

        [Authorize(Roles = "Admin, NhanVienKho")]
        public IActionResult LapPhieuXuat()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, NhanVienKho")]
        public async Task<IActionResult> LapPhieuXuat(PhieuXuat phieuXuat)
        {
            if (ModelState.IsValid)
            {
                // Chuyển việc lưu xuống Database cho Service lo
                await _nhapXuatService.LapPhieuXuatAsync(phieuXuat);

                TempData["SuccessMsg"] = "Đã lập phiếu xuất. Đang chờ Quản lý phê duyệt!";
                return RedirectToAction(nameof(DanhSachPhieuXuat));
            }
            return View(phieuXuat);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> PheDuyetPhieuXuat(int id)
        {
            string nguoiDuyet = User.Identity.Name;

            // Gọi Service để xử lý duyệt và trừ kho
            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, nguoiDuyet);

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

        // =========================================================================
        // KIỂM KÊ KHO
        // =========================================================================
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> KiemKeDinhKy()
        {
            // Gọi Service lấy danh sách vật tư
            var danhSachTonKho = await _nhapXuatService.LayDanhSachKiemKeAsync();
            return View(danhSachTonKho);
        }
    }
}