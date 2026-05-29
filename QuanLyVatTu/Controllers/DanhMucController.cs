using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class DanhMucController : Controller
    {
        private readonly AppDbContext _context;

        public DanhMucController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> VatTu()
        {
            try
            {
                var data = await _context.DanhMucVatTus
                    .AsNoTracking()
                    .OrderBy(x => x.TenDanhMuc)
                    .ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load danh mục: " + ex.Message);
                return View(new List<DanhMucVatTu>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LuuDanhMuc([FromBody] DanhMucVatTu model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ." });
                }

                model.MaDanhMuc = model.MaDanhMuc?.Trim() ?? string.Empty;
                model.TenDanhMuc = model.TenDanhMuc?.Trim() ?? string.Empty;
                model.MoTa = model.MoTa?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(model.MaDanhMuc))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã danh mục." });
                }

                if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên danh mục." });
                }

                var isDuplicateCode = await _context.DanhMucVatTus
                    .AnyAsync(x => x.Id != model.Id && x.MaDanhMuc == model.MaDanhMuc);
                if (isDuplicateCode)
                {
                    return Json(new { success = false, message = "Mã danh mục đã tồn tại." });
                }

                if (model.Id == 0)
                {
                    model.NgayTao = DateTime.Now;
                    model.NguoiTao = User?.Identity?.Name ?? "System";
                    _context.DanhMucVatTus.Add(model);
                }
                else
                {
                    var existing = await _context.DanhMucVatTus.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy danh mục cần cập nhật." });
                    }

                    existing.MaDanhMuc = model.MaDanhMuc;
                    existing.TenDanhMuc = model.TenDanhMuc;
                    existing.MoTa = model.MoTa;
                    existing.TrangThai = model.TrangThai;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhMucById(int id)
        {
            var item = await _context.DanhMucVatTus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy danh mục." });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = item.Id,
                    maDanhMuc = item.MaDanhMuc,
                    tenDanhMuc = item.TenDanhMuc,
                    moTa = item.MoTa,
                    trangThai = (int)item.TrangThai
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> XoaDanhMuc(int id)
        {
            try
            {
                var item = await _context.DanhMucVatTus.FindAsync(id);
                if (item == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục cần xóa." });
                }

                var hasVatTu = await _context.VatTus.AnyAsync(x => x.DanhMucId == id);
                if (hasVatTu)
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục đang có vật tư sử dụng." });
                }

                _context.DanhMucVatTus.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> NhaCungCap()
        {
            try
            {
                var data = await _context.NhaCungCaps
                    .AsNoTracking()
                    .OrderBy(x => x.TenNhaCungCap)
                    .ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load nhà cung cấp: " + ex.Message);
                return View(new List<NhaCungCap>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LuuNhaCungCap([FromBody] NhaCungCap model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ." });
                }

                model.MaNCC = model.MaNCC?.Trim() ?? string.Empty;
                model.TenNhaCungCap = model.TenNhaCungCap?.Trim() ?? string.Empty;
                model.DiaChi = NormalizeOptional(model.DiaChi);
                model.SoDienThoai = NormalizeOptional(model.SoDienThoai);
                model.Email = NormalizeOptional(model.Email);

                if (string.IsNullOrWhiteSpace(model.MaNCC))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã nhà cung cấp." });
                }

                if (string.IsNullOrWhiteSpace(model.TenNhaCungCap))
                {
                    return Json(new { success = false, message = "Vui lòng nhập tên nhà cung cấp." });
                }

                if (!string.IsNullOrWhiteSpace(model.Email) && !new EmailAddressAttribute().IsValid(model.Email))
                {
                    return Json(new { success = false, message = "Email nhà cung cấp không hợp lệ." });
                }

                var isDuplicateCode = await _context.NhaCungCaps
                    .AnyAsync(x => x.Id != model.Id && x.MaNCC == model.MaNCC);
                if (isDuplicateCode)
                {
                    return Json(new { success = false, message = "Mã nhà cung cấp đã tồn tại." });
                }

                if (model.Id == 0)
                {
                    _context.NhaCungCaps.Add(model);
                }
                else
                {
                    var existing = await _context.NhaCungCaps.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy nhà cung cấp cần cập nhật." });
                    }

                    existing.MaNCC = model.MaNCC;
                    existing.TenNhaCungCap = model.TenNhaCungCap;
                    existing.DiaChi = model.DiaChi;
                    existing.SoDienThoai = model.SoDienThoai;
                    existing.Email = model.Email;
                    existing.TrangThai = model.TrangThai;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNhaCungCap(int id)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhà cung cấp này!" });
            }

            // Phải bọc trong thuộc tính 'data' để khớp với JS
            return Json(new { success = true, data = ncc });
        }

        [HttpPost]
        public async Task<IActionResult> XoaNhaCungCap(int id)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null) return Json(new { success = false, message = "Dữ liệu không tồn tại!" });

            // Cảnh báo: Nếu Nhà Cung Cấp này đã có Phiếu Nhập, bạn không nên xóa mà chỉ nên đổi Trạng thái về 0
            _context.NhaCungCaps.Remove(ncc);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private static string? NormalizeOptional(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
