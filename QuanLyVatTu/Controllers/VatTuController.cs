using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    public class VatTuController : Controller
    {
        private readonly AppDbContext _context;

        public VatTuController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // 1. TRANG DANH SÁCH VẬT TƯ (Load giao diện và dữ liệu)
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Index(int? danhMucId, string tuKhoa)
        {
            try
            {
                // 1. Lấy danh sách Danh mục cho Dropdown Lọc
                ViewBag.DanhSachDanhMuc = new SelectList(await _context.DanhMucVatTus.ToListAsync(), "Id", "TenDanhMuc", danhMucId);

                // 2. Truy vấn danh sách Vật tư
                var query = _context.VatTus.Include(v => v.DanhMuc).AsQueryable();

                // Lọc theo Danh mục
                if (danhMucId.HasValue && danhMucId.Value > 0)
                {
                    query = query.Where(v => v.DanhMucId == danhMucId.Value);
                }

                // Lọc theo Từ khóa (Mã hoặc Tên)
                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    tuKhoa = tuKhoa.ToLower();
                    query = query.Where(v => v.MaVatTu.ToLower().Contains(tuKhoa)
                                          || v.TenVatTu.ToLower().Contains(tuKhoa));
                }

                var danhSach = await query.OrderByDescending(v => v.Id).ToListAsync();

                // Trả về chính xác tên file View dsVatTu.cshtml
                return View("dsVatTu", danhSach);
            }
            catch (Exception ex)
            {
                return Content($"LỖI TẢI DANH SÁCH VẬT TƯ: {ex.Message}");
            }
        }

        // ================================================================
        // 2. API LẤY THÔNG TIN 1 VẬT TƯ (Dùng cho Modal Sửa bằng AJAX)
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> LayThongTin(int id)
        {
            var vatTu = await _context.VatTus.FindAsync(id);
            if (vatTu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vật tư này trong hệ thống!" });
            }

            return Json(new { success = true, data = vatTu });
        }

        // ================================================================
        // 3. API THÊM / SỬA VẬT TƯ (Nhận dữ liệu từ AJAX Modal)
        // ================================================================
        [HttpPost]
        [Authorize(Roles = "Quản trị viên, Quản lý kho")] // Chỉ Quản lý mới được sửa danh mục
        public async Task<IActionResult> LuuVatTu([FromBody] VatTu model)
        {
            try
            {
                // Kiểm tra trùng Mã Vật Tư
                bool isDuplicate = await _context.VatTus.AnyAsync(v => v.MaVatTu == model.MaVatTu && v.Id != model.Id);
                if (isDuplicate)
                {
                    return Json(new { success = false, message = "Mã vật tư này đã tồn tại. Vui lòng chọn mã khác!" });
                }

                if (model.Id == 0)
                {
                    // THÊM MỚI
                    // BẢO MẬT: Ép Tồn kho và Giá vốn về 0 khi tạo mới. (Không tin tưởng Client)
                    model.TonKhoHienTai = 0;
                    model.GiaVonBinhQuan = 0;

                    _context.VatTus.Add(model);
                    await LogActionAsync($"Thêm mới vật tư: {model.MaVatTu} - {model.TenVatTu}");
                }
                else
                {
                    // CẬP NHẬT
                    var existingVatTu = await _context.VatTus.FindAsync(model.Id);
                    if (existingVatTu == null) return Json(new { success = false, message = "Dữ liệu không tồn tại!" });

                    existingVatTu.MaVatTu = model.MaVatTu;
                    existingVatTu.TenVatTu = model.TenVatTu;
                    existingVatTu.DanhMucId = model.DanhMucId;
                    existingVatTu.DonViTinh = model.DonViTinh;
                    existingVatTu.TonToiThieu = model.TonToiThieu;

                    // TUYỆT ĐỐI KHÔNG cập nhật Tồn kho và Giá vốn ở đây (Chỉ có Service Nhập Xuất mới được phép đổi)

                    _context.VatTus.Update(existingVatTu);
                    await LogActionAsync($"Cập nhật thông tin vật tư: {model.MaVatTu}");
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thông tin vật tư thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // ================================================================
        // 4. API XÓA VẬT TƯ (AJAX)
        // ================================================================
        [HttpPost]
        [Authorize(Roles = "Quản trị viên")] // Xóa là hành động nguy hiểm, chỉ Admin mới được làm
        public async Task<IActionResult> XoaVatTu(int id)
        {
            try
            {
                var vatTu = await _context.VatTus.FindAsync(id);
                if (vatTu == null)
                    return Json(new { success = false, message = "Không tìm thấy vật tư!" });

                // KIỂM TRA TOÀN VẸN DỮ LIỆU: Nếu Vật tư đã từng phát sinh Nhập/Xuất thì KHÔNG ĐƯỢC XÓA
                bool hasHistory = await _context.ChiTietPhieuNhaps.AnyAsync(c => c.VatTuId == id) ||
                                  await _context.ChiTietPhieuXuats.AnyAsync(c => c.VatTuId == id);

                if (hasHistory)
                {
                    return Json(new { success = false, message = "Không thể xóa! Vật tư này đã có lịch sử Nhập/Xuất kho. Bạn chỉ có thể ngừng theo dõi nó." });
                }

                _context.VatTus.Remove(vatTu);
                await _context.SaveChangesAsync();

                await LogActionAsync($"Xóa vật tư: {vatTu.MaVatTu} - {vatTu.TenVatTu}");

                return Json(new { success = true, message = "Đã xóa vật tư thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }

        // ================================================================
        // HÀM HỖ TRỢ GHI LOG
        // ================================================================
        private async Task LogActionAsync(string action)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int taiKhoanId))
            {
                var log = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = action,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };
                _context.NhatKyHeThongs.Add(log);
                // Không gọi SaveChangesAsync ở đây để hàm gọi nó tự Save (tạo Transaction chung)
            }
        }
    }
}