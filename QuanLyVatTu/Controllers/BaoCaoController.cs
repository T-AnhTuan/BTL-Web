using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;

namespace QuanLyVatTu.Controllers
{
    [Authorize(Roles = "Quản trị viên, Quản lý kho, Admin")]
    public class BaoCaoController : Controller
    {
        private readonly IBaoCaoService _baoCaoService;
        private readonly AppDbContext _context;

        public BaoCaoController(IBaoCaoService baoCaoService, AppDbContext context)
        {
            _baoCaoService = baoCaoService;
            _context = context;
        }

        // =========================================================================
        // TRANG CHỦ BÁO CÁO: TỔNG HỢP XUẤT - NHẬP - TỒN
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] BaoCaoVM filter)
        {
            try
            {
                ViewBag.DanhSachKho = new SelectList(await _context.DanhMucKhos.ToListAsync(), "Id", "TenKho", filter.KhoId);

                // Giá trị hiển thị cho form (ngày nguyên)
                DateTime? uiFrom = filter.TuNgay?.Date;
                DateTime? uiTo = filter.DenNgay?.Date;

                // Giá trị dùng cho truy vấn (toDate inclusive = end of day)
                DateTime fromDate = uiFrom ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime toDateInclusive = (uiTo ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);

                var viewModel = await _baoCaoService.LayBaoCaoNhapXuatTonAsync(filter.KhoId, fromDate, toDateInclusive, filter.TuKhoa)
                                 ?? new BaoCaoVM();

                // Gán lại giá trị hiển thị (nguyên ngày) để form giữ trạng thái
                viewModel.KhoId = filter.KhoId;
                viewModel.TuNgay = uiFrom;
                viewModel.DenNgay = uiTo;
                viewModel.TuKhoa = filter.TuKhoa;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi khi tải báo cáo: {ex.Message}");
            }
        }


        // =========================================================================
        // TRANG THẺ KHO: Lấy dữ liệu chi tiết Nhập - Xuất
        // =========================================================================
        /*[HttpGet]
        public async Task<IActionResult> Kho(int? vatTuId, DateTime? tuNgay, DateTime? denNgay)
        {
            // Code hàm Kho giữ nguyên như bạn đã viết
            try
            {
                ViewBag.VatTus = await _context.VatTus.ToListAsync();
                ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
                ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

                KhoVM model = new KhoVM();

                if (!vatTuId.HasValue || vatTuId.Value <= 0)
                {
                    return View(model);
                }

                var vt = await _context.VatTus.FindAsync(vatTuId.Value);
                if (vt != null)
                {
                    model.VatTuId = vt.Id;
                    model.TenVatTu = vt.TenVatTu;
                    model.TenKho = "Kho Tổng";
                }
                DateTime? realDenNgay = denNgay;
                if (denNgay.HasValue)
                {
                    realDenNgay = denNgay.Value.Date.AddDays(1).AddTicks(-1);
                }
                var dsNhap = await _context.ChiTietPhieuNhaps
                    .Include(c => c.PhieuNhap)
                    .Where(c => c.VatTuId == vatTuId.Value
                                && (!tuNgay.HasValue || c.PhieuNhap.NgayNhap >= tuNgay.Value)
                                && (!realDenNgay.HasValue || c.PhieuNhap.NgayNhap <= realDenNgay.Value))
                    .Select(c => new ChiTietGiaoDichViewModel
                    {
                        NgayGiaoDich = c.PhieuNhap.NgayNhap,
                        SoChungTu = "PN-" + c.PhieuNhap.MaPhieu,
                        DienGiai = "Nhập kho",
                        LoaiGiaoDich = "Nhập",
                        SoLuong = c.SoLuong,
                        GhiChu = c.PhieuNhap.GhiChu
                    }).ToListAsync();

                var dsXuat = await _context.ChiTietPhieuXuats
                    .Include(c => c.PhieuXuat)
                    .Where(c => c.VatTuId == vatTuId.Value
                                && (!tuNgay.HasValue || c.PhieuXuat.NgayXuat >= tuNgay.Value)
                                && (!realDenNgay.HasValue || c.PhieuXuat.NgayXuat <= realDenNgay.Value))
                    .Select(c => new ChiTietGiaoDichViewModel
                    {
                        NgayGiaoDich = c.PhieuXuat.NgayXuat,
                        SoChungTu = "PX-" + c.PhieuXuat.MaPhieu,
                        DienGiai = "Xuất kho",
                        LoaiGiaoDich = "Xuất",
                        SoLuong = c.SoLuong,
                        GhiChu = c.PhieuXuat.LyDoXuat
                    }).ToListAsync();

                var tatCaGiaoDich = dsNhap.Concat(dsXuat).OrderBy(x => x.NgayGiaoDich).ToList();

                int tonDauKy = 0;
                if (tuNgay.HasValue)
                {
                    var nhapTruocDo = await _context.ChiTietPhieuNhaps.Include(c => c.PhieuNhap)
                        .Where(c => c.VatTuId == vatTuId.Value && c.PhieuNhap.NgayNhap < tuNgay.Value)
                        .SumAsync(c => c.SoLuong);

                    var xuatTruocDo = await _context.ChiTietPhieuXuats.Include(c => c.PhieuXuat)
                        .Where(c => c.VatTuId == vatTuId.Value && c.PhieuXuat.NgayXuat < tuNgay.Value)
                        .SumAsync(c => c.SoLuong);

                    tonDauKy = nhapTruocDo - xuatTruocDo;
                }

                int tonHienTai = tonDauKy;
                foreach (var giaoDich in tatCaGiaoDich)
                {
                    if (giaoDich.LoaiGiaoDich == "Nhập") tonHienTai += giaoDich.SoLuong;
                    else tonHienTai -= giaoDich.SoLuong;

                    giaoDich.TonKhoSauGiaoDich = tonHienTai;
                }

                model.ChiTietGiaoDich = tatCaGiaoDich;
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi load thẻ kho: " + ex.Message);
                return View(new KhoVM());
            }*/
    }
}