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
    [Authorize]
    public class VatTuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INhatKyService _nhatKyService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private const decimal MAX_GIA_VON = 999999999M;
        private const long MAX_TON_KHO = 999999999;
        public VatTuController(AppDbContext context, INhatKyService nhatKyService, ITinhGiaVonService tinhGiaVonService)
        {
            _context = context;
            _nhatKyService = nhatKyService;
            _tinhGiaVonService = tinhGiaVonService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? danhMucId, string? tuKhoa)
        {
            try
            {
                ViewBag.DanhMucs = await _context.DanhMucVatTus
                    .AsNoTracking()
                    .OrderBy(d => d.TenDanhMuc)
                    .ToListAsync();
                ViewBag.DanhMucId = danhMucId;
                ViewBag.TuKhoa = tuKhoa;

                var query = _context.VatTus
                    .AsNoTracking()
                    .Include(v => v.DanhMuc)
                    .AsQueryable();

                if (danhMucId.HasValue && danhMucId.Value > 0)
                {
                    query = query.Where(v => v.DanhMucId == danhMucId.Value);
                }

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    var keyword = tuKhoa.Trim();
                    query = query.Where(v => v.MaVatTu.Contains(keyword) || v.TenVatTu.Contains(keyword));
                }

                var data = await query
                    .OrderBy(v => v.TenVatTu)
                    .ToListAsync();

                return View(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi truy vấn Vật tư: " + ex.Message);
                ViewBag.DanhMucs = new List<DanhMucVatTu>();
                return View(new List<VatTu>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var vatTu = await _context.VatTus
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vatTu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu!" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = vatTu.Id,
                    maVatTu = vatTu.MaVatTu,
                    tenVatTu = vatTu.TenVatTu,
                    danhMucId = vatTu.DanhMucId,
                    donViTinh = vatTu.DonViTinh,
                    tonKhoHienTai = vatTu.TonKhoHienTai,
                    tonToiThieu = vatTu.TonToiThieu,
                    giaVonBinhQuan = vatTu.GiaVonBinhQuan
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VatTu model)
        {
            try
            {
                var validationMessage = await ValidateVatTuAsync(model);
                if (validationMessage != null)
                {
                    return Json(new { success = false, message = validationMessage });
                }

                var isExist = await _context.VatTus.AnyAsync(v => v.MaVatTu == model.MaVatTu);
                if (isExist)
                {
                    return Json(new { success = false, message = "Mã vật tư này đã tồn tại trong hệ thống!" });
                }

                _context.VatTus.Add(model);
                await _context.SaveChangesAsync();
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Thêm vật tư mới {model.TenVatTu} (Mã: {model.MaVatTu})",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] VatTu model)
        {
            try
            {
                var validationMessage = await ValidateVatTuAsync(model);
                if (validationMessage != null)
                {
                    return Json(new { success = false, message = validationMessage });
                }

                var existingVatTu = await _context.VatTus.FindAsync(model.Id);
                if (existingVatTu == null)
                {
                    return Json(new { success = false, message = "Vật tư không tồn tại hoặc đã bị xóa!" });
                }

                var isExist = await _context.VatTus
                    .AnyAsync(v => v.Id != model.Id && v.MaVatTu == model.MaVatTu);
                if (isExist)
                {
                    return Json(new { success = false, message = "Mã vật tư mới đã bị trùng!" });
                }

                existingVatTu.MaVatTu = model.MaVatTu;
                existingVatTu.TenVatTu = model.TenVatTu;
                existingVatTu.DanhMucId = model.DanhMucId;
                existingVatTu.DonViTinh = model.DonViTinh;
                existingVatTu.TonKhoHienTai = model.TonKhoHienTai;
                existingVatTu.GiaVonBinhQuan = model.GiaVonBinhQuan;
                existingVatTu.TonToiThieu = model.TonToiThieu;

                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Cập nhật vật tư {model.TenVatTu} (Mã: {model.MaVatTu})",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var vatTu = await _context.VatTus.FindAsync(id);
                if (vatTu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vật tư để xóa!" });
                }

                var hasGiaoDich = await _context.ChiTietPhieuNhaps.AnyAsync(c => c.VatTuId == id)
                    || await _context.ChiTietPhieuXuats.AnyAsync(c => c.VatTuId == id)
                    || await _context.ChiTietKhos.AnyAsync(c => c.VatTuId == id);
                if (hasGiaoDich)
                {
                    return Json(new { success = false, message = "Không thể xóa vật tư đã có phát sinh giao dịch!" });
                }

                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    HanhDong = $"Xóa vật tư {vatTu.TenVatTu} (Mã: {vatTu.MaVatTu})",
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                _context.VatTus.Remove(vatTu);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa: " + ex.Message });
            }
        }

        private async Task<string?> ValidateVatTuAsync(VatTu? model)
        {
            if (model == null)
            {
                return "Dữ liệu gửi lên không hợp lệ.";
            }

            model.MaVatTu = model.MaVatTu?.Trim() ?? string.Empty;
            model.TenVatTu = model.TenVatTu?.Trim() ?? string.Empty;
            model.DonViTinh = model.DonViTinh?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.MaVatTu))
            {
                return "Vui lòng nhập mã vật tư.";
            }

            if (string.IsNullOrWhiteSpace(model.TenVatTu))
            {
                return "Vui lòng nhập tên vật tư.";
            }

            if (model.DanhMucId <= 0 || !await _context.DanhMucVatTus.AnyAsync(d => d.Id == model.DanhMucId))
            {
                return "Danh mục vật tư không hợp lệ.";
            }

            if (model.TonKhoHienTai < 0)
            {
                return "Tồn kho hiện tại không được nhỏ hơn 0.";
            }
            if (model.TonKhoHienTai > MAX_TON_KHO)
            {
                return $"Tồn kho hiện tại không được vượt quá {MAX_TON_KHO:N0}.";
            }
            if (model.TonToiThieu < 0)
            {
                return "Tồn tối thiểu không được nhỏ hơn 0.";
            }
            if (model.TonToiThieu > MAX_TON_KHO)
            {
                return $"Tồn kho tối thiểu không được vượt quá {MAX_TON_KHO:N0}.";
            }
            if (model.GiaVonBinhQuan < 0)
            {
                return "Giá vốn bình quân không được nhỏ hơn 0.";
            }
            if (model.GiaVonBinhQuan > MAX_GIA_VON)
            {
                return $"Giá vốn bình quân không được vượt quá {MAX_GIA_VON:N0} VNĐ.";
            }
            return null;
        }
    }
}
