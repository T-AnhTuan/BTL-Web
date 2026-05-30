using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.Services;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IBaoCaoService _baoCaoService;
        private readonly IThongBaoService _thongBaoService;
        public HomeController(AppDbContext context, IBaoCaoService baoCaoService, IThongBaoService thongBaoService)
        {
            _context = context;
            _baoCaoService = baoCaoService;
            _thongBaoService = thongBaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int taiKhoanId = string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);
                // Thống kê cơ bản
                ViewBag.TongLoaiVatTu = await _context.VatTus
           .AsNoTracking()
           .Select(v => v.MaVatTu)        // đổi thành trường mã thực tế của bạn
           .Where(m => !string.IsNullOrEmpty(m))
           .Distinct()
           .CountAsync();

                // Tính tổng giá trị tồn kho (lấy minimal fields)
                var totalValuePerCode = await _context.VatTus
             .AsNoTracking()
             .GroupBy(v => v.MaVatTu)
             .Select(g => new
             {
                 Code = g.Key,
                 TotalQty = g.Sum(x => (decimal?)x.TonKhoHienTai) ?? 0m,
                 TotalValue = g.Sum(x => (decimal?)(x.TonKhoHienTai * x.GiaVonBinhQuan)) ?? 0m
             })
             .ToListAsync();
                var totalInventoryValue = totalValuePerCode.Sum(x => x.TotalValue);
                ViewBag.TongGiaTriKho = totalInventoryValue;

                // Cảnh báo & phiếu chờ
                ViewBag.CanhBaoTonKho = await _context.VatTus.AsNoTracking()
                    .CountAsync(v => v.TonKhoHienTai <= v.TonToiThieu);

                ViewBag.PhieuNhapChoPheDuyet = await _context.PhieuNhaps.AsNoTracking()
                    .CountAsync(p => p.TrangThai == TrangThaiPhieuNhap.ChoDuyet);

                ViewBag.PhieuXuatChoPheDuyet = await _context.PhieuXuats.AsNoTracking()
                    .CountAsync(p => p.TrangThai == TrangThaiPhieuXuat.ChoDuyet);


                var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

                // 1. Dữ liệu cảnh báo tồn kho (Dưới định mức)
                var lowStock = await _context.VatTus.AsNoTracking()
                    .Where(v => v.TonKhoHienTai <= (v.TonToiThieu)) // Giả sử dưới định mức min
                    .Select(v => new { MaVatTu = v.MaVatTu, TenVatTu = v.TenVatTu, SoLuong = v.TonKhoHienTai, DinhMuc = v.TonToiThieu })
                    .Take(5).ToListAsync();
                ViewBag.LowStockJson = JsonSerializer.Serialize(lowStock, jsonOptions);

                // 2. Dữ liệu thông báo hoạt động (Trộn Nhập & Xuất)
                var nhaps = await _context.PhieuNhaps
                    .AsNoTracking()
                    .OrderByDescending(p => p.NgayNhap).Take(10)
                    .Select(p => new { 
                        ThoiGian = p.NgayNhap,
                        Loai = "Nhập Kho", 
                        NoiDung = "Tạo phiếu nhập mã " + p.MaPhieu, 
                        NguoiThucHien = "Admin" })
                    .ToListAsync();

                var xuats = await _context.PhieuXuats
                    .AsNoTracking()
                    .OrderByDescending(p => p.NgayXuat).Take(10)
                    .Select(p => new { 
                        ThoiGian = p.NgayXuat,
                        Loai = "Xuất Kho",
                        NoiDung = "Tạo phiếu xuất mã " + p.MaPhieu, 
                        NguoiThucHien = p.NguoiXuat??"Admin" })
                    .ToListAsync();

                var activities = nhaps.Concat(xuats).OrderByDescending(a => a.ThoiGian).Take(10).ToList();
                ViewBag.ActivitiesJson = JsonSerializer.Serialize(activities, jsonOptions);

                // 3. Mock Data Biểu đồ (Bạn có thể tự thay bằng dữ liệu thực)
                var labels = new List<string>();
                var nhapData = new List<int>();
                var xuatData = new List<int>();

                // Tìm ngày Thứ 2 đầu tuần (trong trường hợp hôm nay là Chủ nhật thì DayOfWeek là 0, cần xử lý)
                DateTime today = DateTime.Today;
                int diff = (7 + (int)today.DayOfWeek - 1) % 7;
                DateTime startOfWeek = today.AddDays(-1 * diff);

                for (int i = 0; i < 7; i++)
                {
                    DateTime date = startOfWeek.AddDays(i);
                    labels.Add($"{GetDayName(date.DayOfWeek)} ({date:dd/MM})");

                    // Đếm/Tổng hợp dữ liệu theo ngày thực tế
                    int slNhap = _context.PhieuNhaps
                        .Where(p => p.NgayNhap.Date == date.Date && p.TrangThai == TrangThaiPhieuNhap.DaDuyet)
                        .Sum(p => p.ChiTietPhieuNhaps.Sum(ct => ct.SoLuong));

                    int slXuat = _context.PhieuXuats
                        .Where(p => p.NgayXuat.Date == date.Date && p.TrangThai == TrangThaiPhieuXuat.DaDuyet)
                        .Sum(p => p.ChiTietPhieuXuats.Sum(ct => ct.SoLuong));

                    nhapData.Add(slNhap);
                    xuatData.Add(slXuat);
                }


                ViewBag.TrendLabelsJson = JsonSerializer.Serialize(labels, jsonOptions);
                ViewBag.TrendInJson = JsonSerializer.Serialize(nhapData, jsonOptions);
                ViewBag.TrendOutJson = JsonSerializer.Serialize(xuatData, jsonOptions);

                // 4. Mock Data Bieu do san pham
                var categoryStats = await _context.VatTus.AsNoTracking()
                 .Include(v => v.DanhMuc) // Kết nối qua bảng Danh Mục
                 .Where(v => v.DanhMuc != null)
                 .GroupBy(v => v.DanhMuc.TenDanhMuc) // Nhóm theo Tên Danh Mục
                 .Select(g => new
                 {
                     CategoryName = g.Key,
                     Count = g.Count() // Đếm số lượng Vật tư trong từng danh mục
                 })
                 .OrderByDescending(g => g.Count) // Xếp giảm dần (Top nhiều nhất lên đầu)
                 .Take(5) // Cắt lấy 5 danh mục nhiều nhất (tránh biểu đồ bị vụn)
                 .ToListAsync();
                var catLabels = categoryStats.Select(c => c.CategoryName).ToArray();
                var catValues = categoryStats.Select(c => c.Count).ToArray();

                ViewBag.CategoryLabelsJson = JsonSerializer.Serialize(catLabels, jsonOptions);
                ViewBag.CategoryValuesJson = JsonSerializer.Serialize(catValues, jsonOptions);
                return View();
            }
            catch (Exception ex) // Nếu có bất kỳ lỗi gì xảy ra ở khối Try (sai kết nối, biến null...)
            {
                // Thay vì sập trang Error, sẽ in thẳng nguyên nhân lỗi ra màn hình để lập trình viên sửa
                return Content($"LỖI TẠI HOME CONTROLLER: {ex.Message} \nChi tiết: {ex.InnerException?.Message}");
            }
        }

        [HttpGet]
        public IActionResult BaoTri()
        {
            return View();
        }
        private string GetDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                _ => "CN"
            };
        }
    }
}