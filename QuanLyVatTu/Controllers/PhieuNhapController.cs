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

        public PhieuNhapController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context;
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
        [HttpGet]
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

        [HttpGet]
        public async Task<IActionResult> XoaPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps.Include(p => p.ChiTietPhieuNhaps).FirstOrDefaultAsync(p => p.Id == id);
            if (phieu == null) return RedirectToAction("PhieuNhap");

            if (phieu.TrangThai == TrangThaiPhieuNhap.DaDuyet)
            {
                TempData["ErrorMsg"] = "Không thể xóa phiếu đã duyệt!";
                return RedirectToAction("PhieuNhap");
            }

            if (phieu.ChiTietPhieuNhaps != null) _context.ChiTietPhieuNhaps.RemoveRange(phieu.ChiTietPhieuNhaps);
            _context.PhieuNhaps.Remove(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = $"Đã xóa phiếu {phieu.MaPhieu}!";
            return RedirectToAction("PhieuNhap");
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
}