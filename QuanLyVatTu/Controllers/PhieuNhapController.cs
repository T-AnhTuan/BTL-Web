using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;

namespace QuanLyVatTu.Controllers
{
    public class PhieuNhapController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly AppDbContext _context;
        private readonly ILogger<PhieuNhapController> _logger;
        public PhieuNhapController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context,
            ILogger<PhieuNhapController> logger
            )
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context; 
            _logger = logger;
        }

        // === 1. GET: DANH SÁCH ===
        [HttpGet]
        public async Task<IActionResult> PhieuNhap(DateTime? tuNgay, DateTime? denNgay, int? khoId, int? nhaCungCapId)
        {
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.Where(n => n.TrangThai == TrangThaiNhaCungCap.Active).ToListAsync();
            ViewBag.Khos = await _context.DanhMucKhos.Where(k => k.TrangThai == TrangThaiKho.Active).ToListAsync();

            var query = _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
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

        // === 4. LƯU HOÀN CHỈNH (HEADER + DETAILS CÙNG LÚC) LÊN SERVER ===
        [HttpPost]
        public async Task<IActionResult> LuuPhieuNhapHoanChinh([FromBody] PhieuNhapPayload data)
        {
            try
            {
                PhieuNhap phieu;
                decimal tongTien = 0;

                // NẾU LÀ TẠO MỚI (Id = 0)
                if (data.Id == 0)
                {
                    phieu = new PhieuNhap
                    {
                        MaPhieu = data.MaPhieu,
                        NgayNhap = data.NgayNhap,
                        KhoId = data.KhoId,
                        NhaCungCapId = data.NhaCungCapId,
                        GhiChu = data.GhiChu,
                        TrangThai = TrangThaiPhieuNhap.ChoDuyet
                    };
                    _context.PhieuNhaps.Add(phieu);
                    await _context.SaveChangesAsync(); // Lưu để lấy ID
                }
                else // NẾU LÀ CẬP NHẬT PHIẾU CŨ
                {
                    phieu = await _context.PhieuNhaps.FindAsync(data.Id);
                    if (phieu == null) return Json(new { success = false, message = "Không tìm thấy phiếu gốc." });

                    if (phieu.TrangThai == TrangThaiPhieuNhap.DaDuyet)
                        return Json(new { success = false, message = "Phiếu đã duyệt không thể sửa!" });

                    // Xóa chi tiết cũ đi để thêm lại lưới mới
                    var chiTietCu = _context.ChiTietPhieuNhaps.Where(c => c.PhieuNhapId == phieu.Id);
                    _context.ChiTietPhieuNhaps.RemoveRange(chiTietCu);
                }

                // THÊM LƯỚI VẬT TƯ VÀO DB
                if (data.ChiTiet != null)
                {
                    foreach (var item in data.ChiTiet)
                    {
                        _context.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                        {
                            PhieuNhapId = phieu.Id,
                            VatTuId = item.VatTuId,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        });
                        tongTien += (item.SoLuong * item.DonGia);
                    }
                }

                // Cập nhật tổng tiền cho Header
                phieu.TongGiaTri = tongTien;
                _context.PhieuNhaps.Update(phieu);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lưu phiếu nhập thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // === 5. CÁC HÀM DUYỆT / XÓA  ===
        [HttpPost]
        [Authorize(Roles = "Admin , Manager, Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps.Include(p => p.ChiTietPhieuNhaps).FirstOrDefaultAsync(p => p.Id == id);
            if (phieu == null) return RedirectToAction("PhieuNhap");

            if (phieu.ChiTietPhieuNhaps == null || !phieu.ChiTietPhieuNhaps.Any())
            {
                TempData["ErrorMsg"] = "Lỗi: Phiếu trống chưa có vật tư, không thể duyệt!";
                return RedirectToAction("PhieuNhap");
            }

            phieu.TrangThai = TrangThaiPhieuNhap.DaDuyet;
            _context.PhieuNhaps.Update(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = $"Đã duyệt phiếu {phieu.MaPhieu} thành công!";
            return RedirectToAction("PhieuNhap");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> XemChiTiet(int id)
        {
            _logger.LogInformation("XemChiTiet called with id={Id}", id);
            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu)
                .Include(p => p.Kho)
                .Include(p => p.NhaCungCap)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null)
            {
                _logger.LogWarning("XemChiTiet: phieu null id={Id}", id);
                return NotFound(); // tạm thời trả NotFound để dễ thấy status 404
            }

            return View("XemChiTiet", phieu);
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

            try
            {
                phieu.TrangThai = TrangThaiPhieuNhap.TuChoi;

                // Nếu model có trường lưu lý do từ chối, gán vào đây
                // Ví dụ: phieu.LyDoTuChoi = lyDo;
                // Nếu không có, bỏ hoặc thêm migration tương ứng

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
        public async Task<IActionResult> LuuToanBoPhieu([FromBody] PhieuNhapToanBoDto payload)
        {
            try
            {
                // 1. Kiểm tra dữ liệu an toàn
                if (payload == null || payload.ChiTiets == null || !payload.ChiTiets.Any())
                {
                    return Json(new { success = false, message = "Lưới rỗng! Vui lòng chọn ít nhất 1 vật tư." });
                }

                if (payload.KhoId == 0 || payload.NhaCungCapId == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn Kho và Nhà cung cấp!" });
                }

                // 2. Tạo mã phiếu tự động nếu chưa có (VD: PN_20260529_161300)
                string maPhieuMoi = string.IsNullOrEmpty(payload.MaPhieu)
                                    ? $"PN_{DateTime.Now:yyyyMMdd_HHmmss}"
                                    : payload.MaPhieu;

                // 3. Khởi tạo Phiếu Nhập gốc (Header)
                var phieuMoi = new PhieuNhap
                {
                    MaPhieu = maPhieuMoi,
                    NgayNhap = payload.NgayNhap != default ? payload.NgayNhap : DateTime.Now,
                    KhoId = payload.KhoId,
                    NhaCungCapId = payload.NhaCungCapId,
                    GhiChu = payload.GhiChu,
                    TrangThai = TrangThaiPhieuNhap.ChoDuyet, // Mặc định là chờ duyệt
                    TongGiaTri = 0 // Sẽ tính ở bước dưới
                };// Thêm vào DB để lấy ID (Lưu ý: chưa SaveChanges ngay)
                _context.PhieuNhaps.Add(phieuMoi);

                // 4. Khởi tạo danh sách Chi Tiết Phiếu Nhập
                decimal tongTien = 0;
                phieuMoi.ChiTietPhieuNhaps = new List<ChiTietPhieuNhap>();

                foreach (var item in payload.ChiTiets)
                {
                    var ct = new ChiTietPhieuNhap
                    {
                        VatTuId = item.VatTuId,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    };
                    phieuMoi.ChiTietPhieuNhaps.Add(ct); // Gắn chi tiết vào phiếu gốc

                    tongTien += (item.SoLuong * item.DonGia);
                }

                // Cập nhật lại tổng tiền cho phiếu gốc
                phieuMoi.TongGiaTri = tongTien;

                // 5. Lưu toàn bộ (Cả Cha và Con) vào Database trong 1 transaction duy nhất của EF Core
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = $"Đã lưu thành công phiếu nhập {maPhieuMoi}!";
                return Json(new { success = true, message = "Lưu phiếu thành công!", redirectUrl = "/PhieuNhap/PhieuNhap" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }

    // --- CÁC LỚP DTO HỨNG DỮ LIỆU ---
    public class PhieuNhapTaoMoiDto
    {
        public DateTime NgayNhap { get; set; }
        public int KhoId { get; set; }
        public int NhaCungCapId { get; set; }
        public string? GhiChu { get; set; }
    }

    public class PhieuNhapPayload
    {
        public int Id { get; set; }
        public string MaPhieu { get; set; }
        public DateTime NgayNhap { get; set; }
        public int KhoId { get; set; }
        public int NhaCungCapId { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTietDto>? ChiTiet { get; set; }
    }

    public class ChiTietDto
    {
        public int VatTuId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
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