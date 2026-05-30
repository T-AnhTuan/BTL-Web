using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Route("PhieuNhap/[action]")]
    [Authorize]
    public class PhieuNhapController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly INhatKyService _nhatKyService;
        private readonly AppDbContext _context;
        public PhieuNhapController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context,
            INhatKyService nhatKyService
            )
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context;
            _nhatKyService = nhatKyService;
        }

        // === 1. GET: DANH SÁCH ===
        [HttpGet]
        public async Task<IActionResult> PhieuNhap(DateTime? tuNgay, DateTime? denNgay, int? khoId, int? nhaCungCapId)
        {
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.Where(n => n.TrangThai == TrangThaiNhaCungCap.Active).ToListAsync();
            ViewBag.Khos = await _context.DanhMucKhos.Where(k => k.TrangThai == TrangThaiKho.Active).ToListAsync();

            var query = _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps!)
                    .ThenInclude(c => c.VatTu)
                .Include(p => p.Kho)
                .AsQueryable();

            if (tuNgay.HasValue) query = query.Where(p => p.NgayNhap >= tuNgay.Value.Date);
            if (denNgay.HasValue) query = query.Where(p => p.NgayNhap <= denNgay.Value.Date.AddDays(1).AddTicks(-1));
            if (khoId.HasValue && khoId > 0) query = query.Where(p => p.KhoId == khoId);
            if (nhaCungCapId.HasValue && nhaCungCapId > 0) query = query.Where(p => p.NhaCungCapId == nhaCungCapId);

            return View(await query.OrderByDescending(p => p.NgayNhap).ToListAsync());
        }

        // === 2. BƯỚC ĐỆM: MANG DỮ LIỆU TỪ MODAL SANG TRANG CHI TIẾT (CHƯA LƯU DB) ===
        [HttpPost]
        public async Task<IActionResult> ChuyenTiepChiTiet(PhieuNhapTaoMoiDto dto)
        {
            // Tạo một phiếu tạm trong bộ nhớ (RAM)
            var phieuTam = new PhieuNhap
            {
                Id = 0, // Id = 0 để dấu hiệu cho JS biết đây là tạo mới
                MaPhieu = "PN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                NgayNhap = dto.NgayNhap,
                KhoId = dto.KhoId,
                NhaCungCapId = dto.NhaCungCapId,
                GhiChu = dto.GhiChu,
                TongGiaTri = 0,
                TrangThai = TrangThaiPhieuNhap.ChoDuyet
            };

            // Lấy tên Kho và NCC để hiển thị ra View
            phieuTam.Kho = await _context.DanhMucKhos.FindAsync(dto.KhoId);
            phieuTam.NhaCungCap = await _context.NhaCungCaps.FindAsync(dto.NhaCungCapId);

            ViewBag.VatTus = await _context.VatTus.ToListAsync();

            // Render luôn trang Chi Tiết với dữ liệu tạm này
            return View("ChiTietPhieuNhap", phieuTam);
        }

        // === 3. XEM LẠI CHI TIẾT PHIẾU ĐÃ LƯU ===
        [HttpGet]
        public async Task<IActionResult> ChiTietPhieuNhap(int id)
        {
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .Include(p => p.ChiTietPhieuNhaps!)
                    .ThenInclude(c => c.VatTu)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieuNhap == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu nhập!";
                return RedirectToAction("PhieuNhap");
            }

            ViewBag.VatTus = await _context.VatTus.ToListAsync();
            return View(phieuNhap);
        }

        // === 5. CÁC HÀM DUYỆT  ===
        [HttpPost]
        [Authorize(Roles = "Admin , Manager, Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.VatTu) // nếu không có navigation, bỏ dòng này và dùng query bên dưới
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null) return RedirectToAction("PhieuNhap");

            if (phieu.ChiTietPhieuNhaps == null || !phieu.ChiTietPhieuNhaps.Any())
            {
                TempData["ErrorMsg"] = "Lỗi: Phiếu trống chưa có vật tư, không thể duyệt!";
                return RedirectToAction("PhieuNhap");
            }

            // Nếu phiếu đã ở trạng thái ĐãDuyet thì không làm lại
            if (phieu.TrangThai == TrangThaiPhieuNhap.DaDuyet)
            {
                TempData["InfoMsg"] = $"Phiếu {phieu.MaPhieu} đã ở trạng thái Đã duyệt.";
                return RedirectToAction("PhieuNhap");
            }

            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    phieu.TrangThai = TrangThaiPhieuNhap.DaDuyet;
                    foreach (var line in phieu.ChiTietPhieuNhaps)
                    {
                         var vatTu = await _context.VatTus
                            .FirstOrDefaultAsync(v => v.Id == line.VatTuId);

                        if (vatTu != null)
                        {
                            decimal tongGiaTriTonCu=vatTu.TonKhoHienTai * vatTu.GiaVonBinhQuan;
                            decimal tongGiaTriNhapMoi = line.SoLuong * line.DonGia;
                            int tonKhoMoi = vatTu.TonKhoHienTai + line.SoLuong;
                            if (tonKhoMoi > 0)
                            {
                                vatTu.GiaVonBinhQuan = (tongGiaTriTonCu + tongGiaTriNhapMoi) / tonKhoMoi;
                            }
                            vatTu.TonKhoHienTai = tonKhoMoi;
                            _context.VatTus.Update(vatTu);
                        }
                        else throw new InvalidOperationException($"Không tìm thấy VatTu Id={line.VatTuId}");
                      var entry = new NhatKyHeThong
                      {
                          TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                          HanhDong = $"Duyệt phiếu nhập {phieu.MaPhieu}",
                          DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                          ThoiGian = DateTime.Now
                      };
                        await _nhatKyService.GhiNhatKyAsync(entry);

                    }
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    TempData["SuccessMsg"] = $"Đã duyệt phiếu {phieu.MaPhieu} và cập nhật tồn kho.";
                    phieu.TrangThai = TrangThaiPhieuNhap.DaDuyet;

                    _context.PhieuNhaps.Update(phieu);
                    
                    return RedirectToAction("PhieuNhap");
                    
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    TempData["ErrorMsg"] = "Lỗi khi duyệt phiếu: " + ex.Message;
                    return RedirectToAction("PhieuNhap");
                }
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> XemChiTiet(int id)
        {
            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu)
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null)
            {
                return NotFound(); 
            }

            return View(phieu);
        }

        [Authorize(Roles = "Admin,Manager,Quản trị viên,Quản lý kho")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            // Không cho từ chối nếu đã duyệt
            if (phieu.TrangThai == TrangThaiPhieuNhap.DaDuyet)
            {
                TempData["ErrorMsg"] = "Không thể từ chối phiếu đã duyệt!";
                return RedirectToAction(nameof(PhieuNhap));
            }
            var entry = new NhatKyHeThong
            {
                TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                HanhDong = $"Từ chối phiếu nhập {phieu.MaPhieu}",
                DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            };
            await _nhatKyService.GhiNhatKyAsync(entry);
            try
            {
                phieu.TrangThai = TrangThaiPhieuNhap.TuChoi;

                _context.PhieuNhaps.Update(phieu);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = $"Đã từ chối phiếu {phieu.MaPhieu}.";
            }
            catch (Exception ex)
            {
                // Tốt nhất inject ILogger<YourController> và log ex
                TempData["ErrorMsg"] = "Có lỗi khi từ chối phiếu: " + ex.Message;
            }

            return RedirectToAction(nameof(PhieuNhap));
        }

        [HttpPost]
        public async Task<IActionResult> LuuToanBoPhieu([FromBody] PhieuNhapToanBoDto model)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                await LogActionAsync(userId, "Vừa tạp phiếu nhập " + model.MaPhieu);
            }
            try
            {
                // BƯỚC CHẶN 1: Bắt lỗi nếu JS gửi dữ liệu sai định dạng (Ví dụ chữ gửi vào số)
                if (!ModelState.IsValid)
                {
                    // Trích xuất các lỗi của C# ra thành 1 chuỗi văn bản
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Dữ liệu gửi lên sai định dạng C#: " + string.Join(", ", errors) });
                }

                // Tạo phiếu nhập mới
                var phieuMoi = new PhieuNhap
                {
                    MaPhieu = string.IsNullOrEmpty(model.MaPhieu) ? "PN" + DateTime.Now.ToString("yyyyMMddHHmmss") : model.MaPhieu,
                    NgayNhap = model.NgayNhap,
                    KhoId = model.KhoId,
                    NhaCungCapId = model.NhaCungCapId,
                    GhiChu = model.GhiChu,
                    TrangThai = TrangThaiPhieuNhap.ChoDuyet,
                    TongGiaTri = 0 // Sẽ cộng dồn ở dưới
                };

                _context.PhieuNhaps.Add(phieuMoi);
                await _context.SaveChangesAsync(); // Lưu để lấy ID gốc

                decimal tongTien = 0;

                // Lưu danh sách chi tiết vật tư
                foreach (var item in model.ChiTiets)
                {
                    var ct = new ChiTietPhieuNhap
                    {
                        PhieuNhapId = phieuMoi.Id,
                        VatTuId = item.VatTuId,
                        SoLuong = item.SoLuong, // C# đang yêu cầu số nguyên (int)
                        DonGia = item.DonGia
                    };
                    tongTien += (item.SoLuong * item.DonGia);
                    _context.ChiTietPhieuNhaps.Add(ct);
                }

                phieuMoi.TongGiaTri = tongTien;
                await _context.SaveChangesAsync();
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Tạo phiếu nhập {phieuMoi.MaPhieu}",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                return Json(new
                {
                    success = true,
                    message = "Lưu phiếu thành công!",
                    redirectUrl = $"/PhieuNhap/XemChiTiet/{phieuMoi.Id}" // Trả về đường dẫn để JS tự nhảy trang
                });

            }
            catch (Exception ex)
            {
                string errInfo = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = $"Lỗi hệ thống C#: {errInfo}" });
            }
        }

        // --- CÁC LỚP DTO HỨNG DỮ LIỆU ---
        private async Task LogActionAsync(int taiKhoanId, string action)
        {
            try
            {
                var log = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = action,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };
                _context.NhatKyHeThongs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
            }
        }
        public class PhieuNhapTaoMoiDto
        {
            public DateTime NgayNhap { get; set; }
            public int KhoId { get; set; }
            public int NhaCungCapId { get; set; }
            public string? GhiChu { get; set; }
        }
        public class ItemVatTuDto
        {
            public int VatTuId { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
        }
        public class PhieuNhapToanBoDto
        {
            // Thông tin chung (Header)
            public string? MaPhieu { get; set; } // Có thể rỗng, Server sẽ tự sinh
            public DateTime NgayNhap { get; set; }
            public int KhoId { get; set; }
            public int NhaCungCapId { get; set; }
            public string? GhiChu { get; set; }

            // Danh sách vật tư (Details)
            public List<ItemVatTuDto> ChiTiets { get; set; }
        }
    }
}