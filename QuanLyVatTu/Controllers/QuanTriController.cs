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
        public async Task<IActionResult> LuuPhanQuyen(int VaiTroId, List<PhanQuyenUpdateDto> dsQuyen) // Hàm nhận vào ID Vai Trò và Danh sách các quyền được tích từ HTML
        {
            try // Bắt đầu khối lệnh thử nghiệm để bắt lỗi hệ thống nếu có
            {
                // 1. Kiểm tra vai trò
                var vaiTro = await _context.VaiTros.FindAsync(VaiTroId); // Tìm Vai Trò trong Database dựa theo VaiTroId gửi lên
                if (vaiTro == null) // Nếu Vai Trò không tồn tại trong Database
                {
                    TempData["ErrorMessage"] = "Lỗi: Không tìm thấy vai trò cần cập nhật!"; // Ghi lại câu thông báo lỗi
                    return RedirectToAction(nameof(PhanQuyen)); // Load lại trang phân quyền
                }

                // 2. Kiểm tra dữ liệu danh sách quyền gửi lên
                if (dsQuyen == null || dsQuyen.Count == 0) // Nếu danh sách quyền bị rỗng (HTML không gửi lên được do sai tên biến)
                {
                    TempData["ErrorMessage"] = "Lỗi: Không nhận được dữ liệu phân quyền từ giao diện!"; // Ghi lại thông báo lỗi
                    return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); // Load lại trang phân quyền của vai trò đó
                }

                // 3. Cập nhật từng dòng quyền vào cơ sở dữ liệu
                foreach (var quyenMoi in dsQuyen) // Dùng vòng lặp chạy qua từng dòng quyền mà giao diện vừa gửi lên
                {
                    var quyenCu = await _context.PhanQuyens.FindAsync(quyenMoi.Id); // Tìm dòng phân quyền tương ứng ở trong Database bằng ID

                    if (quyenCu != null) // Nếu dòng phân quyền này có tồn tại trong Database
                    {
                        quyenCu.CoQuyenXem = quyenMoi.CoQuyenXem;   // Cập nhật quyền Xem: Nếu giao diện gửi lên true thì gán true, không gửi thì mặc định là false
                        quyenCu.CoQuyenThem = quyenMoi.CoQuyenThem; // Cập nhật quyền Thêm: Ghi đè giá trị từ giao diện vào CSDL
                        quyenCu.CoQuyenSua = quyenMoi.CoQuyenSua;   // Cập nhật quyền Sửa: Ghi đè giá trị từ giao diện vào CSDL
                        quyenCu.CoQuyenXoa = quyenMoi.CoQuyenXoa;   // Cập nhật quyền Xóa: Ghi đè giá trị từ giao diện vào CSDL
                    }
                }

                // 4. Lưu xuống CSDL và thông báo
                await _context.SaveChangesAsync(); // Chạy lệnh lưu tất cả các thay đổi vừa làm ở trên xuống CSDL thật
                TempData["SuccessMessage"] = $"Lưu cấu hình phân quyền cho [{vaiTro.TenVaiTro}] thành công!"; // Tạo ra câu thông báo thành công màu xanh

                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); // Điều hướng load lại trang phân quyền của đúng vai trò đó
            }
            catch (Exception ex) // Nếu có bất kỳ lỗi nào xảy ra trong quá trình chạy (ví dụ: đứt kết nối CSDL)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu cấu hình: " + ex.Message; // Bắt lấy lỗi đó và ghi vào thông báo
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); // Load lại trang và hiển thị lỗi ra
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaoMoiVaiTro([FromBody] VaiTroMoiDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenVaiTro))
                    return Json(new { success = false, message = "Tên vai trò không được để trống!" });

                // Kiểm tra trùng lặp
                bool tonTai = await _context.VaiTros.AnyAsync(v => v.TenVaiTro.Trim().ToLower() == model.TenVaiTro.Trim().ToLower());
                if (tonTai)
                    return Json(new { success = false, message = "Tên vai trò này đã tồn tại trong hệ thống!" });

                // 1. Tạo vai trò mới
                var vaiTroMoi = new VaiTro
                {
                    TenVaiTro = model.TenVaiTro,
                    MoTa = model.MoTa
                };
                _context.VaiTros.Add(vaiTroMoi);
                await _context.SaveChangesAsync(); // Lưu để lấy ID mới

                // 2. KHỞI TẠO MA TRẬN QUYỀN MẶC ĐỊNH (Tất cả = false)
                // Lấy danh sách các chức năng hiện có trong hệ thống (từ một vai trò mẫu, ví dụ Admin)
                var danhSachChucNang = await _context.PhanQuyens
                    .Select(p => p.TenChucNang)
                    .Distinct()
                    .ToListAsync();

                // Tạo các dòng dữ liệu quyền cho Vai trò vừa tạo
                foreach (var chucNang in danhSachChucNang)
                {
                    _context.PhanQuyens.Add(new Models.PhanQuyen
                    {
                        VaiTroId = vaiTroMoi.Id,
                        TenChucNang = chucNang,
                        CoQuyenXem = false,
                        CoQuyenThem = false,
                        CoQuyenSua = false,
                        CoQuyenXoa = false
                    });
                }

                // Lưu ma trận quyền
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm vai trò và khởi tạo ma trận quyền thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
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
        public class VaiTroMoiDto
        {
            public string TenVaiTro { get; set; }
            public string MoTa { get; set; }
        }
    }
}