using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.ViewModels; 

namespace QuanLyVatTu.Controllers
{
    // Cấp quyền Admin cho toàn bộ Controller này
    [Authorize(Roles = "Admin, Quản trị viên")]
    public class QuanTriController : Controller
    {
        private readonly AppDbContext _context;

        public QuanTriController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // QUẢN LÝ TÀI KHOẢN - GIAO DIỆN
        // ==========================================
        public async Task<IActionResult> QuanLyTaiKhoan()
        {
            ViewBag.VaiTros = await _context.VaiTros
                .AsNoTracking()
                .OrderBy(v => v.TenVaiTro)
                .ToListAsync();

            var taiKhoans = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.NhanVien)
                .Include(t => t.VaiTro)
                .OrderBy(t => t.TenDangNhap)
                .ToListAsync();

            return View(taiKhoans);
        }

        // ==========================================
        // QUẢN LÝ TÀI KHOẢN - CÁC HÀM API (Dành cho Javascript)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetTaiKhoanById(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taiKhoan == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = taiKhoan.Id,
                    tenDangNhap = taiKhoan.TenDangNhap,
                    hoTen = taiKhoan.NhanVien?.HoTen,
                    vaiTroId = taiKhoan.VaiTroId,
                    trangThai = (int)taiKhoan.TrangThai
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> LuuTaiKhoan([FromBody] TaiKhoanCrudDto model)
        {
            try
            {
                if (model.Id == 0) // THÊM MỚI
                {
                    bool isExist = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap);
                    if (isExist) return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

                    var newNhanVien = new NhanVien
                    {
                        MaNV = "NV" + DateTime.Now.Ticks.ToString().Substring(10),
                        HoTen = model.HoTen ?? "Người dùng mới"
                    };
                    _context.NhanViens.Add(newNhanVien);
                    await _context.SaveChangesAsync();

                    var newTaiKhoan = new TaiKhoan
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                        VaiTroId = model.VaiTroId,
                        TrangThai = (TrangThaiTaiKhoan)model.TrangThai,
                        NhanVienId = newNhanVien.Id
                    };
                    _context.TaiKhoans.Add(newTaiKhoan);
                }
                else // CẬP NHẬT
                {
                    var taiKhoan = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(t => t.Id == model.Id);
                    if (taiKhoan == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });

                    // Cập nhật thông tin nhân viên
                    if (taiKhoan.NhanVien != null)
                    {
                        taiKhoan.NhanVien.HoTen = model.HoTen;
                    }

                    // Cập nhật tài khoản
                    taiKhoan.VaiTroId = model.VaiTroId;
                    taiKhoan.TrangThai = (TrangThaiTaiKhoan)model.TrangThai;

                    // Nếu có nhập pass mới thì mới đổi
                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> KhoaMoTaiKhoan(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản" });

            if (tk.TenDangNhap.ToLower() == "admin")
                return Json(new { success = false, message = "Không được phép khóa tài khoản Admin quản trị cao nhất!" });

            tk.TrangThai = tk.TrangThai == TrangThaiTaiKhoan.Active ? TrangThaiTaiKhoan.Locked : TrangThaiTaiKhoan.Active;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            var tk = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(t => t.Id == id);
            if (tk == null) return Json(new { success = false, message = "Không tìm thấy tài khoản" });

            if (tk.TenDangNhap.ToLower() == "admin")
                return Json(new { success = false, message = "Không được phép xóa tài khoản Admin quản trị cao nhất!" });

            try
            {
                _context.TaiKhoans.Remove(tk);
                if (tk.NhanVien != null) _context.NhanViens.Remove(tk.NhanVien);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException)
            {
                tk.TrangThai = TrangThaiTaiKhoan.Locked;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = "Tài khoản đã có dữ liệu giao dịch. Đã tự động chuyển sang trạng thái: KHÓA." });
            }
        }

        // ==========================================
        // QUẢN LÝ PHÂN QUYỀN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PhanQuyen(int? vaiTroId)
        {
            // 1. Lấy danh sách tất cả các vai trò hiển thị ở Menu trái
            var danhSachVaiTro = await _context.VaiTros.ToListAsync();

            // Nếu DB chưa có vai trò nào, trả về View rỗng để không bị sập trang
            if (danhSachVaiTro == null || !danhSachVaiTro.Any())
            {
                TempData["ErrorMessage"] = "Chưa có vai trò nào trong hệ thống. Vui lòng thêm vai trò mới.";
                return View(new PhanQuyenVM { DanhSachVaiTro = new List<VaiTro>() });
            }

            // 2. Chốt chặn Null: Nếu click từ menu vào (vaiTroId == null), tự động chọn vai trò đầu tiên
            int selectedId = vaiTroId ?? danhSachVaiTro.First().Id;

            // 3. Lấy chi tiết vai trò đang chọn KÈM THEO danh sách phân quyền của nó
            var vaiTroDangChon = await _context.VaiTros
                .Include(v => v.PhanQuyens) // Bắt buộc phải có Include để không bị lỗi Null reference ở bảng bên phải
                .FirstOrDefaultAsync(v => v.Id == selectedId);

            // Xử lý ngoại lệ: Nếu ai đó cố tình gõ URL một cái ID không tồn tại
            if (vaiTroDangChon == null)
            {
                vaiTroDangChon = await _context.VaiTros
                    .Include(v => v.PhanQuyens)
                    .FirstOrDefaultAsync(v => v.Id == danhSachVaiTro.First().Id);
            }
            if (vaiTroDangChon != null && (vaiTroDangChon.PhanQuyens == null || !vaiTroDangChon.PhanQuyens.Any()))
            {
                var cacModuleMacDinh = new List<PhanQuyen>
                {
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "QL_DANHMUC", TenChucNang = "Quản lý Danh Mục Vật Tư", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "QL_NHAPKHO", TenChucNang = "Quản lý Phiếu Nhập Kho", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "QL_XUATKHO", TenChucNang = "Quản lý Phiếu Xuất Kho", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "BAOCAO", TenChucNang = "Báo Cáo Xuất Nhập Tồn", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "HT_TAIKHOAN", TenChucNang = "Quản lý Tài Khoản Hệ Thống", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "HT_PHANQUYEN", TenChucNang = "Thiết lập Phân Quyền", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false }
                };

                // Thêm vào Database và lưu lại
                _context.PhanQuyens.AddRange(cacModuleMacDinh);
                await _context.SaveChangesAsync();

                // Truy vấn lại để lấy dữ liệu mới nhất (có chứa ID của PhanQuyen) ném ra View
                vaiTroDangChon = await _context.VaiTros
                    .Include(v => v.PhanQuyens)
                    .FirstOrDefaultAsync(v => v.Id == vaiTroDangChon.Id);
            }
            // 4. Đóng gói vào ViewModel và ném ra View
            var model = new PhanQuyenVM
            {
                DanhSachVaiTro = danhSachVaiTro,
                VaiTroDangChon = vaiTroDangChon
            };

            return View(model);
        }
        // PHÂN QUYỀN - POST (Lưu dữ liệu cấu hình)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuPhanQuyen(int VaiTroId, List<PhanQuyenUpdateDto> PhanQuyens)
        {
            try
            {
                var vaiTro = await _context.VaiTros
                    .Include(v => v.PhanQuyens)
                    .FirstOrDefaultAsync(v => v.Id == VaiTroId);

                if (vaiTro == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy vai trò cần lưu.";
                    return RedirectToAction(nameof(PhanQuyen));
                }

                // Lưu dữ liệu cấu hình phân quyền
                foreach (var quyenMoi in PhanQuyens)
                {
                    var quyenCu = vaiTro.PhanQuyens.FirstOrDefault(q => q.Id == quyenMoi.Id);
                    if (quyenCu != null)
                    {
                        quyenCu.CoQuyenXem = quyenMoi.CoQuyenXem;
                        quyenCu.CoQuyenThem = quyenMoi.CoQuyenThem;
                        quyenCu.CoQuyenSua = quyenMoi.CoQuyenSua;
                        quyenCu.CoQuyenXoa = quyenMoi.CoQuyenXoa;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Lưu cấu hình phân quyền cho [{vaiTro.TenVaiTro}] thành công!";

                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu cấu hình: " + ex.Message;
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId });
            }
        }

        public class TaiKhoanCrudDto
        {
            public int Id { get; set; }
            public string? TenDangNhap { get; set; }
            public string? MatKhau { get; set; }
            public string? HoTen { get; set; }
            public int VaiTroId { get; set; }
            public int TrangThai { get; set; }
        }
        public class PhanQuyenUpdateDto
        {
            public int Id { get; set; }
            public bool CoQuyenXem { get; set; }
            public bool CoQuyenThem { get; set; }
            public bool CoQuyenSua { get; set; }
            public bool CoQuyenXoa { get; set; }
        }
    }
}