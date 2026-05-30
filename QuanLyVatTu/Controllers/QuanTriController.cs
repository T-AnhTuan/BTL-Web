using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Authorize(Roles = "Admin, Quản trị viên")]
    public class QuanTriController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INhatKyService _nhatKyService;

        public QuanTriController(AppDbContext context, INhatKyService nhatKyService)
        {
            _context = context;
            _nhatKyService = nhatKyService;
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
                if (model.Id == 0) 
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
                    var entry = new NhatKyHeThong
                    {
                        TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                        HanhDong = $"Cấp tài khoản cho {model.HoTen}",
                        DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        ThoiGian = DateTime.Now
                    };
                    await _nhatKyService.GhiNhatKyAsync(entry);
                }
                else
                {
                    var taiKhoan = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(t => t.Id == model.Id);
                    if (taiKhoan == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });

                    if (taiKhoan.NhanVien != null)
                    {
                        taiKhoan.NhanVien.HoTen = model.HoTen;
                    }


                    taiKhoan.VaiTroId = model.VaiTroId;
                    taiKhoan.TrangThai = (TrangThaiTaiKhoan)model.TrangThai;
                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
                    }
                    var entry = new NhatKyHeThong
                    {
                        TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                        HanhDong = $"Thay đổi thông tin tài khoản của {model.HoTen} ",
                        DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        ThoiGian = DateTime.Now
                    };
                    await _nhatKyService.GhiNhatKyAsync(entry);
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
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                HanhDong = $"Xóa tài khoản của {tk.NhanVien.HoTen}",
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            };
            await _nhatKyService.GhiNhatKyAsync(entry);
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
                    new PhanQuyen { VaiTroId = vaiTroDangChon.Id, MaChucNang = "QL_VATTU", TenChucNang = "Quản lý Vật Tư", CoQuyenXem=false, CoQuyenThem=false, CoQuyenSua=false, CoQuyenXoa=false },
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
        public async Task<IActionResult> LuuPhanQuyen(int VaiTroId, List<PhanQuyenUpdateDto> dsQuyen)
        {
            try // Bắt đầu khối lệnh thử nghiệm để bắt lỗi hệ thống nếu có
            {
                // 1. Kiểm tra vai trò
                var vaiTro = await _context.VaiTros.FindAsync(VaiTroId); 
                if (vaiTro == null) 
                {
                    TempData["ErrorMessage"] = "Lỗi: Không tìm thấy vai trò cần cập nhật!"; 
                    return RedirectToAction(nameof(PhanQuyen)); 
                }

                // 2. Kiểm tra dữ liệu danh sách quyền gửi lên
                if (dsQuyen == null || dsQuyen.Count == 0) 
                {
                    TempData["ErrorMessage"] = "Lỗi: Không nhận được dữ liệu phân quyền từ giao diện!";
                    return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); 
                }

                foreach (var quyenMoi in dsQuyen)
                {
                    var quyenCu = await _context.PhanQuyens.FindAsync(quyenMoi.Id); 

                    if (quyenCu != null) 
                    {
                        quyenCu.CoQuyenXem = quyenMoi.CoQuyenXem;  
                        quyenCu.CoQuyenThem = quyenMoi.CoQuyenThem; 
                        quyenCu.CoQuyenSua = quyenMoi.CoQuyenSua;   
                        quyenCu.CoQuyenXoa = quyenMoi.CoQuyenXoa;   
                    }
                }

                // 4. Lưu xuống CSDL và thông báo
                await _context.SaveChangesAsync(); 
                TempData["SuccessMessage"] = $"Lưu cấu hình phân quyền cho [{vaiTro.TenVaiTro}] thành công!"; 
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Cấp quyền {vaiTro.TenVaiTro} cho tài khoản {User.Identity.Name}",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); 
            }
            catch (Exception ex) 
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi lưu cấu hình: " + ex.Message; 
                return RedirectToAction(nameof(PhanQuyen), new { vaiTroId = VaiTroId }); 
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
                await _context.SaveChangesAsync(); 

                // 2. KHỞI TẠO MA TRẬN QUYỀN MẶC ĐỊNH (Tất cả = false)
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