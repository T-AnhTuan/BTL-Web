using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class NhapXuatController : Controller
    {
        private readonly INhapXuatService _nhapXuatService;
        private readonly ITinhGiaVonService _tinhGiaVonService;
        private readonly AppDbContext _context;

        public NhapXuatController(
            INhapXuatService nhapXuatService,
            ITinhGiaVonService tinhGiaVonService,
            AppDbContext context)
        {
            _nhapXuatService = nhapXuatService;
            _tinhGiaVonService = tinhGiaVonService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> PhieuNhap(DateTime? tuNgay, DateTime? denNgay, int? khoId, int? nhaCungCapId)
        {
            // 1. Lấy dữ liệu cho các Dropdown (Dùng cho cả form tạo mới và bộ lọc)
            ViewBag.NhaCungCaps = await _context.NhaCungCaps
                .AsNoTracking()
                .Where(n => n.TrangThai == TrangThaiNhaCungCap.Active)
                .OrderBy(n => n.TenNhaCungCap)
                .ToListAsync();

            ViewBag.Khos = await _context.DanhMucKhos
                .AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKho.Active)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            // 2. Query cơ sở dữ liệu
            var query = _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
                .Include(p => p.Kho)
                .AsQueryable();

            // 3. Xử lý Logic lọc tự động
            if (tuNgay.HasValue)
            {
                query = query.Where(p => p.NgayNhap >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                // Cộng thêm đến 23:59:59 của ngày kết thúc để lấy trọn vẹn ngày đó
                var toDate = denNgay.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.NgayNhap <= toDate);
            }

            if (khoId.HasValue && khoId > 0)
            {
                query = query.Where(p => p.KhoId == khoId);
            }

            if (nhaCungCapId.HasValue && nhaCungCapId > 0)
            {
                query = query.Where(p => p.NhaCungCapId == nhaCungCapId);
            }

            // Trả lại các giá trị lọc ra View để hiển thị lại trên ô input
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.KhoId = khoId;
            ViewBag.NhaCungCapId = nhaCungCapId;
            // 4. Trả kết quả ra View
            var danhSach = await query.OrderByDescending(p => p.NgayNhap).ToListAsync();
            return View(danhSach);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuNhap(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            var phieu = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu nhập.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.TrangThai != TrangThaiPhieuNhap.ChoDuyet)
            {
                TempData["ErrorMsg"] = "Chỉ có thể duyệt phiếu nhập đang chờ duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.ChiTietPhieuNhaps == null || !phieu.ChiTietPhieuNhaps.Any())
            {
                TempData["ErrorMsg"] = "Phiếu nhập chưa có vật tư, không thể duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            phieu.TrangThai = TrangThaiPhieuNhap.DaDuyet;
            await _context.SaveChangesAsync();

            var updatedCost = await _tinhGiaVonService.TinhGiaVonBinhQuanSauKhiNhapAsync(id, taiKhoanId);
            TempData[updatedCost ? "SuccessMsg" : "ErrorMsg"] = updatedCost
                ? "Đã duyệt phiếu nhập và cập nhật tồn kho/giá vốn."
                : "Đã duyệt phiếu nhập nhưng cập nhật tồn kho/giá vốn thất bại.";

            return RedirectToAction(nameof(PhieuNhap));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> HuyPhieuNhap(int id)
        {
            var phieu = await _context.PhieuNhaps.FindAsync(id);
            if (phieu == null)
            {
                TempData["ErrorMsg"] = "Không tìm thấy phiếu nhập.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            if (phieu.TrangThai != TrangThaiPhieuNhap.ChoDuyet)
            {
                TempData["ErrorMsg"] = "Chỉ có thể hủy phiếu nhập đang chờ duyệt.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            phieu.TrangThai = TrangThaiPhieuNhap.TuChoi;
            await _context.SaveChangesAsync();
            TempData["SuccessMsg"] = "Đã hủy phiếu nhập thành công!";

            return RedirectToAction(nameof(PhieuNhap));
        }
        [HttpGet]
        public async Task<IActionResult> ChiTietPhieuNhap(int id)
        {
            // Tìm phiếu nhập theo ID, bắt buộc phải móc nối (Include) chi tiết và vật tư để tránh lỗi rỗng
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(c => c.VatTu)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Nếu tìm không thấy ID, báo lỗi và đẩy về trang danh sách
            if (phieuNhap == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu nhập này hoặc phiếu đã bị xóa.";
                return RedirectToAction(nameof(PhieuNhap));
            }

            // Đổ danh sách Vật tư lên ViewBag để người dùng chọn trong Dropdown lúc ấn "Thêm chi tiết"
            ViewBag.VatTus = await _context.VatTus
                .Select(v => new {
                    v.Id,
                    v.MaVatTu,
                    v.TenVatTu,
                    v.DonViTinh
                })
                .OrderBy(v => v.TenVatTu)
                .ToListAsync();

            return View(phieuNhap); // Trả phiếu nhập kèm chi tiết ra View
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChiTietPhieuNhap(PhieuNhap phieuNhap)
        {
            try
            {
                // Kiểm tra đối tượng không null
                if (phieuNhap == null)
                {
                    TempData["ErrorMsg"] = "Thông tin phiếu nhập không hợp lệ.";
                    return RedirectToAction(nameof(PhieuNhap));
                }

                // Lấy thông tin người dùng hiện tại
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
                {
                    TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                    return RedirectToAction(nameof(PhieuNhap));
                }

                // Chuẩn hóa dữ liệu đầu vào
                phieuNhap.MaPhieu = phieuNhap.MaPhieu?.Trim() ?? string.Empty;
                phieuNhap.GhiChu = phieuNhap.GhiChu?.Trim() ?? string.Empty;

                // Tự động tạo mã phiếu nếu chưa có
                if (string.IsNullOrEmpty(phieuNhap.MaPhieu))
                {
                    phieuNhap.MaPhieu = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                // KIỂM TRA CHỈ CÁC TRƯỜNG BẮTBUỘC (Ko check ModelState vì nó validate cả navigation properties)
                if (string.IsNullOrWhiteSpace(phieuNhap.MaPhieu))
                {
                    TempData["ErrorMsg"] = "Mã phiếu nhập không hợp lệ.";
                    return RedirectToAction(nameof(PhieuNhap));
                }

                if (phieuNhap.NhaCungCapId <= 0)
                {
                    TempData["ErrorMsg"] = "Vui lòng chọn nhà cung cấp.";
                    return RedirectToAction(nameof(PhieuNhap));
                }

                if (phieuNhap.KhoId <= 0)
                {
                    TempData["ErrorMsg"] = "Vui lòng chọn kho nhập.";
                    return RedirectToAction(nameof(PhieuNhap));
                }

                // Validate dữ liệu đầy đủ
                var validationMessage = await ValidatePhieuNhapAsync(phieuNhap);
                if (validationMessage != null)
                {
                    TempData["ErrorMsg"] = validationMessage;
                    return RedirectToAction(nameof(PhieuNhap));
                }

                // Gọi service để tạo phiếu nhập
                var result = await _nhapXuatService.LapPhieuNhapAsync(phieuNhap, taiKhoanId);

                if (result.IsSuccess)
                {
                    TempData["SuccessMsg"] = "Đã tạo thông tin Phiếu Nhập thành công! Vui lòng thêm vật tư.";
                    // Lấy ID của phiếu vừa tạo để chuyển đến form chi tiết
                    var newPhieu = await _context.PhieuNhaps
                        .Where(p => p.MaPhieu == phieuNhap.MaPhieu)
                        .FirstOrDefaultAsync();
                    if (newPhieu != null)
                    {
                        return RedirectToAction(nameof(ChiTietPhieuNhap), new { id = newPhieu.Id });
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = result.Message;
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMsg"] = $"Lỗi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Lỗi khi tạo phiếu nhập: {ex.Message}";
            }

            return RedirectToAction(nameof(PhieuNhap));
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChiTietPhieuNhap([FromBody] ChiTietPhieuNhap chiTiet)
        {
            // Kiểm tra các trường dữ liệu gửi lên xem có hợp lệ không (ID > 0, Số lượng > 0)
            if (chiTiet.PhieuNhapId == 0 || chiTiet.VatTuId == 0 || chiTiet.SoLuong <= 0 || chiTiet.DonGia < 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
            }

            try
            {
                // Kiểm tra xem Vật tư này đã có trong Phiếu Nhập này chưa?
                var existChiTiet = await _context.ChiTietPhieuNhaps
                    .FirstOrDefaultAsync(c => c.PhieuNhapId == chiTiet.PhieuNhapId && c.VatTuId == chiTiet.VatTuId);

                if (existChiTiet != null)
                {
                    // Nếu có rồi, cộng dồn số lượng và cập nhật đơn giá mới
                    existChiTiet.SoLuong += chiTiet.SoLuong;
                    existChiTiet.DonGia = chiTiet.DonGia;
                    _context.Update(existChiTiet);
                }
                else
                {
                    // Nếu chưa có, thêm mới dòng này vào Database
                    _context.ChiTietPhieuNhaps.Add(chiTiet);
                }

                // Lưu thực tế xuống CSDL
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm vật tư thành công!" }); // Trả JSON cho JS biết thành công
            }
            catch (Exception ex)
            {
                // Nếu văng lỗi máy chủ, báo lại cho JS
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChiTietPhieuNhap(int id)
        {
            try
            {
                // Tìm dòng chi tiết trong DB theo ID
                var chiTiet = await _context.ChiTietPhieuNhaps.FindAsync(id);
                if (chiTiet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dữ liệu để xóa." });
                }

                // Ra lệnh xóa dòng đó đi
                _context.ChiTietPhieuNhaps.Remove(chiTiet);
                // Cập nhật sự thay đổi xuống CSDL
                await _context.SaveChangesAsync();

                // Trả tín hiệu về cho JS để JS tải lại trang
                return Json(new { success = true, message = "Xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PhieuXuat()

        {
            ViewBag.Khos = await _context.DanhMucKhos
                .AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKho.Active)
                .OrderBy(k => k.TenKho)
                .ToListAsync();

            var danhSach = await _context.PhieuXuats
                .AsNoTracking()
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuXuats)
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();

            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChiTietPhieuXuat(PhieuXuat phieuXuat)
        {
            phieuXuat.MaPhieu = phieuXuat.MaPhieu?.Trim() ?? string.Empty;
            phieuXuat.KhachHang = phieuXuat.KhachHang?.Trim() ?? string.Empty;
            phieuXuat.NguoiXuat = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";
            phieuXuat.LyDoXuat = phieuXuat.LyDoXuat?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(phieuXuat.MaPhieu))
            {
                phieuXuat.MaPhieu = "PX-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            var validationMessage = await ValidatePhieuXuatAsync(phieuXuat);
            if (validationMessage != null)
            {
                TempData["ErrorMsg"] = validationMessage;
                return RedirectToAction(nameof(PhieuXuat));
            }

            phieuXuat.TrangThai = TrangThaiPhieuXuat.ChoDuyet;
            _context.PhieuXuats.Add(phieuXuat);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Tạo thông tin Phiếu Xuất thành công. Hãy thêm vật tư cần xuất!";
            return RedirectToAction(nameof(PhieuXuat));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")]
        public async Task<IActionResult> DuyetPhieuXuat(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int taiKhoanId))
            {
                TempData["ErrorMsg"] = "Phiên đăng nhập không hợp lệ.";
                return RedirectToAction(nameof(PhieuXuat));
            }

            var result = await _nhapXuatService.PheDuyetPhieuXuatAsync(id, taiKhoanId);
            if (result.IsSuccess)
            {
                TempData["SuccessMsg"] = result.Message;
            }
            else
            {
                TempData["ErrorMsg"] = result.Message;
            }

            return RedirectToAction(nameof(PhieuXuat));
        }

        private async Task<string?> ValidatePhieuNhapAsync(PhieuNhap phieuNhap)
        {
            if (string.IsNullOrWhiteSpace(phieuNhap.MaPhieu))
            {
                return "Mã phiếu nhập không hợp lệ.";
            }

            if (await _context.PhieuNhaps.AnyAsync(p => p.MaPhieu == phieuNhap.MaPhieu))
            {
                return "Mã phiếu nhập đã tồn tại.";
            }

            if (!await _context.NhaCungCaps.AnyAsync(n => n.Id == phieuNhap.NhaCungCapId && n.TrangThai == TrangThaiNhaCungCap.Active))
            {
                return "Nhà cung cấp không hợp lệ hoặc đã tạm ngừng.";
            }

            if (!await _context.DanhMucKhos.AnyAsync(k => k.Id == phieuNhap.KhoId && k.TrangThai == TrangThaiKho.Active))
            {
                return "Kho nhập không hợp lệ hoặc đã tạm ngừng.";
            }

            return null;
        }

        private async Task<string?> ValidatePhieuXuatAsync(PhieuXuat phieuXuat)
        {
            if (string.IsNullOrWhiteSpace(phieuXuat.MaPhieu))
            {
                return "Mã phiếu xuất không hợp lệ.";
            }

            if (string.IsNullOrWhiteSpace(phieuXuat.KhachHang))
            {
                return "Vui lòng nhập khách hàng/công trình.";
            }

            if (await _context.PhieuXuats.AnyAsync(p => p.MaPhieu == phieuXuat.MaPhieu))
            {
                return "Mã phiếu xuất đã tồn tại.";
            }

            if (!await _context.DanhMucKhos.AnyAsync(k => k.Id == phieuXuat.KhoId && k.TrangThai == TrangThaiKho.Active))
            {
                return "Kho xuất không hợp lệ hoặc đã tạm ngừng.";
            }

            return null;
        }
    }
}
