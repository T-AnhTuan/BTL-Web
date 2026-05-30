using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Route("PhieuXuat/[action]")]
    [Authorize]
    public class PhieuXuatController : Controller
    {
        // Khai báo biến chứa các Service và Context để tương tác với Database
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly AppDbContext _context;
        private readonly INhatKyService _nhatKyService;

        // Hàm khởi tạo (Constructor): Tự động nạp (inject) các Service và DbContext vào Controller
        public PhieuXuatController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            INhatKyService nhatKyService,
            AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _nhatKyService = nhatKyService;
            _context = context;
        }

        // 1. TRANG DANH SÁCH PHIẾU XUẤT (Phương thức GET)
        [HttpGet]
        public async Task<IActionResult> PhieuXuat(DateTime? tuNgay, DateTime? denNgay, int? khoId, int? khachHangId)
        {
            // Truy vấn danh sách các Kho đang hoạt động để đẩy ra Dropdown ở giao diện
            ViewBag.Khos = await _context.DanhMucKhos
                .Where(k => k.TrangThai == TrangThaiKho.Active)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            var query = _context.PhieuXuats
                .Include(p => p.Kho)
                .AsQueryable();

            if (tuNgay.HasValue) query = query.Where(p => p.NgayXuat >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(p => p.NgayXuat <= denNgay.Value.Date.AddDays(1).AddTicks(-1));
            if (khoId.HasValue && khoId > 0) query = query.Where(p => p.KhoId == khoId);
            return View(await query.OrderByDescending(p => p.NgayXuat).ToListAsync());
        }

        // 2. BƯỚC ĐỆM: MANG DỮ LIỆU TỪ MODAL SANG TRANG CHI TIẾT (CHƯA LƯU DB)
        [HttpPost]
        public async Task<IActionResult> ChuyenTiepChiTiet(PhieuXuatTaoMoiDto dto)
        {
            // ✅ LẤY TaiKhoanId TỪ USER HIỆN TẠI (User.Identity.Name - đã là string)
            string taiKhoanId = User?.Identity?.Name ?? "";

            if (string.IsNullOrEmpty(taiKhoanId))
            {
                TempData["ErrorMsg"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("PhieuXuat");
            }

            // Tạo một phiếu tạm trong bộ nhớ (Id = 0 để JS/View biết là tạo mới)
            var phieuTam = new PhieuXuat
            {
                Id = 0,
                MaPhieu = "PX" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                NgayXuat = dto.NgayXuat,
                KhoId = dto.KhoId,
                KhachHang = dto.KhachHang,
                TaiKhoanId = taiKhoanId,
                NguoiXuat = taiKhoanId,   // ✅ Cùng giá trị
                LyDoXuat = dto.LyDoXuat,
                TongTien = 0,
                TrangThai = TrangThaiPhieuXuat.ChoDuyet
            };

            // Lấy tên Kho để hiển thị ra View (nếu cần)
            phieuTam.Kho = await _context.DanhMucKhos.FindAsync(dto.KhoId);

            // Đẩy danh sách vật tư ra ViewBag để làm dropdown chọn vật tư trong giao diện ChiTietPhieuXuat
            ViewBag.VatTus = await _context.VatTus
                .OrderBy(v => v.TenVatTu)
                .ToListAsync();

            // Render luôn trang ChiTietPhieuXuat với dữ liệu tạm này
            return View("ChiTietPhieuXuat", phieuTam);
        }

        // 6. XỬ LÝ DUYỆT PHIẾU
        [HttpPost]
        [Authorize(Roles = "Admin , Manager, Quản trị viên, Quản lý kho")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetPhieuXuat(int id)
        {
            var phieu = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(ct => ct.VatTu) // nếu không có navigation, bỏ dòng này và dùng query bên dưới
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null) return RedirectToAction("PhieuXuat");

            if (phieu.ChiTietPhieuXuats == null || !phieu.ChiTietPhieuXuats.Any())
            {
                TempData["ErrorMsg"] = "Lỗi: Phiếu trống chưa có vật tư, không thể duyệt!";
                return RedirectToAction("PhieuNhap");
            }

            // Nếu phiếu đã ở trạng thái ĐãDuyet thì không làm lại
            if (phieu.TrangThai == TrangThaiPhieuXuat.DaDuyet)
            {
                TempData["InfoMsg"] = $"Phiếu {phieu.MaPhieu} đã ở trạng thái Đã duyệt.";
                return RedirectToAction("PhieuXuat");
            }
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                HanhDong = $"Duyệt phiếu xuất {phieu.MaPhieu}",
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            };
            await _nhatKyService.GhiNhatKyAsync(entry);
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cập nhật tồn cho từng dòng
                    foreach (var line in phieu.ChiTietPhieuXuats)
                    {
                        var vatTu = await _context.VatTus.FirstOrDefaultAsync(v => v.Id == line.VatTuId);

                        if (vatTu == null)
                        {
                            // Nếu không tìm thấy vật tư, rollback và báo lỗi
                            throw new InvalidOperationException($"Không tìm thấy VatTu Id={line.VatTuId}");
                        }

                        var addQty = Convert.ToInt32(line.SoLuong);

                        vatTu.TonKhoHienTai -= addQty;
                        _context.VatTus.Update(vatTu);
                    }

                    // Lưu thay đổi tồn
                    await _context.SaveChangesAsync();

                    // Sau khi cập nhật tồn thành công, set trạng thái phiếu
                    phieu.TrangThai = TrangThaiPhieuXuat.DaDuyet;
                    _context.PhieuXuats.Update(phieu);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    TempData["SuccessMsg"] = $"Đã duyệt phiếu {phieu.MaPhieu} và cập nhật tồn kho.";
                    return RedirectToAction("PhieuXuat");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    TempData["ErrorMsg"] = "Lỗi khi duyệt phiếu: " + ex.Message;
                    return RedirectToAction("PhieuXuat");
                }
            }
        }

        // 7. XỬ LÝ TỪ CHỐI PHIẾU
        [Authorize(Roles = "Admin,Manager,Quản trị viên,Quản lý kho")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiPhieuXuat(int id)
        {
            var phieu = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(PhieuXuat));
            }
            // Không cho từ chối nếu đã duyệt
            if (phieu.TrangThai == TrangThaiPhieuXuat.DaDuyet)
            {
                TempData["ErrorMsg"] = "Không thể từ chối phiếu đã duyệt!";
                return RedirectToAction(nameof(PhieuXuat));
            }
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                HanhDong = $"Từ chối phiếu xuất {phieu.MaPhieu}",
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            };
            await _nhatKyService.GhiNhatKyAsync(entry);
            try
            {
                phieu.TrangThai = TrangThaiPhieuXuat.TuChoi;

                _context.PhieuXuats.Update(phieu);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = $"Đã từ chối phiếu {phieu.MaPhieu}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Có lỗi khi từ chối phiếu: " + ex.Message;
            }

            return RedirectToAction(nameof(PhieuXuat));
        }
        // 5. MÀN HÌNH CHỈ XEM (ĐỂ QUẢN LÝ DUYỆT HOẶC IN)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> XemChiTiet(int id)
        {

            var phieu = await _context.PhieuXuats
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats)
                    .ThenInclude(c => c.VatTu) // Nối tiếp sang bảng Vật Tư để lấy Tên Vật Tư
                .FirstOrDefaultAsync(p => p.Id == id);

            // Nếu không có trả về lỗi 404
            if (phieu == null) return NotFound();

            // Đẩy dữ liệu ra View
            return View(phieu);
        }

        // 3. MÀN HÌNH NHẬP LIỆU CHI TIẾT (Phương thức GET)
        [HttpGet]
        public async Task<IActionResult> ChiTietPhieuXuat(int id)
        {
            var phieu = await _context.PhieuXuats
                .Include(p => p.Kho)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Nếu không tìm thấy, báo lỗi 404
            if (phieu == null) return NotFound("Không tìm thấy phiếu xuất.");

            // Lấy danh sách Vật tư đang hoạt động ném ra ViewBag để làm Dropdown chọn vật tư
            ViewBag.VatTus = await _context.VatTus
                .OrderBy(v => v.TenVatTu)
                .ToListAsync();

            // Trả dữ liệu phiếu gốc ra giao diện
            return View(phieu);
        }

        // 4. API LƯU CHI TIẾT TỪ JAVASCRIPT GỬI LÊN (Phương thức POST nhận chuỗi JSON)
        [HttpPost]
        public async Task<IActionResult> LuuToanBoPhieu([FromBody] PhieuXuatToanBoDto model)
        {
            try
            {
                // BƯỚC CHẶN 1: Bắt lỗi nếu JS gửi dữ liệu sai định dạng (Ví dụ chữ gửi vào số)
                if (!ModelState.IsValid)
                {
                    // Trích xuất các lỗi của C# ra thành 1 chuỗi văn bản
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Dữ liệu gửi lên sai định dạng C#: " + string.Join(", ", errors) });
                }

                // ✅ LẤY TaiKhoanId TỪ USER HIỆN TẠI (User.Identity.Name - đã là string)
                string taiKhoanId = User?.Identity?.Name ?? "";

                if (string.IsNullOrEmpty(taiKhoanId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại." });
                }

                // Tạo phiếu xuất mới
                var phieuMoi = new PhieuXuat
                {
                    MaPhieu = string.IsNullOrEmpty(model.MaPhieu) ? "PX" + DateTime.Now.ToString("yyyyMMddHHmmss") : model.MaPhieu,
                    NgayXuat = model.NgayXuat,
                    KhoId = model.KhoId,
                    KhachHang = model.KhachHang,
                    LyDoXuat = model.LyDoXuat,
                    TaiKhoanId = taiKhoanId,  // ✅ GÁN TỪ User.Identity.Name
                    TrangThai = TrangThaiPhieuXuat.ChoDuyet,
                    TongTien = 0 // Sẽ cộng dồn ở dưới
                };

                _context.PhieuXuats.Add(phieuMoi);
                await _context.SaveChangesAsync(); // Lưu để lấy ID gốc

                decimal tongTien = 0;

                // Lưu danh sách chi tiết vật tư
                foreach (var item in model.ChiTiets)
                {
                    var ct = new ChiTietPhieuXuat
                    {
                        PhieuXuatId = phieuMoi.Id,
                        VatTuId = item.VatTuId,
                        SoLuong = item.SoLuong, // C# đang yêu cầu số nguyên (int)
                        DonGiaXuat = item.DonGia
                    };
                    tongTien += (item.SoLuong * item.DonGia);
                    _context.ChiTietPhieuXuats.Add(ct);
                }

                phieuMoi.TongTien = tongTien;
                await _context.SaveChangesAsync();
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Tạo phiếu xuất mới {phieuMoi.MaPhieu}",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                return Json(new
                {
                    success = true,
                    message = "Lưu phiếu thành công!",
                    redirectUrl = $"/PhieuXuat/XemChiTiet/{phieuMoi.Id}" // Trả về đường dẫn để JS tự nhảy trang
                });
            }
            catch (Exception ex)
            {
                string errInfo = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                // BƯỚC CHẶN 2: Luôn trả về Json dù lỗi nặng thế nào đi nữa
                return Json(new { success = false, message = $"Lỗi hệ thống C#: {errInfo}" });
            }
        }

        public class PhieuXuatTaoMoiDto
        {
            public DateTime NgayXuat { get; set; }
            public int KhoId { get; set; }
            public string KhachHang { get; set; }
            public string? NguoiXuat { get; set; }
            public string? LyDoXuat { get; set; }
        }
        public class ItemVatTuDto
        {
            public int VatTuId { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
        }
        public class PhieuXuatToanBoDto
        {
            // Thông tin chung (Header)
            public string? MaPhieu { get; set; } // Có thể rỗng, Server sẽ tự sinh
            public DateTime NgayXuat { get; set; }
            public int KhoId { get; set; }
            public string KhachHang { get; set; }
            public string? LyDoXuat { get; set; }

            // Danh sách vật tư (Details)
            public List<ItemVatTuDto> ChiTiets { get; set; }
        }
    }
}