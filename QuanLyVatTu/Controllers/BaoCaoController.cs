using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        // SỬA LỖI 3: Dùng thẳng BaoCaoVM làm parameter để map tự động với Form
        public async Task<IActionResult> Index(BaoCaoVM filter)
        {
            try
            {
                ViewBag.DanhSachKho = new SelectList(await _context.DanhMucKhos.ToListAsync(), "Id", "TenKho", filter.KhoId);

                // Thiết lập ngày mặc định nếu người dùng chưa chọn
                DateTime fromDate = filter.TuNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime toDate = filter.DenNgay ?? DateTime.Now.Date;

                // Gọi Service lấy dữ liệu
                var viewModel = await _baoCaoService.LayBaoCaoNhapXuatTonAsync(filter.KhoId, fromDate, toDate, filter.TuKhoa);

                // Quan trọng: Gắn lại các giá trị lọc vào ViewModel để giao diện không bị mất giá trị khi load lại
                viewModel.KhoId = filter.KhoId;
                viewModel.TuNgay = fromDate;
                viewModel.DenNgay = toDate;
                viewModel.TuKhoa = filter.TuKhoa;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi khi tải báo cáo: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PhanTichThongKe()
        {
            return View();
        }

        // =========================================================================
        // TRANG THẺ KHO: Lấy dữ liệu chi tiết Nhập - Xuất
        // =========================================================================
        [HttpGet]
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

                var dsNhap = await _context.ChiTietPhieuNhaps
                    .Include(c => c.PhieuNhap)
                    .Where(c => c.VatTuId == vatTuId.Value
                                && (!tuNgay.HasValue || c.PhieuNhap.NgayNhap >= tuNgay.Value)
                                && (!denNgay.HasValue || c.PhieuNhap.NgayNhap <= denNgay.Value))
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
                                && (!denNgay.HasValue || c.PhieuXuat.NgayXuat <= denNgay.Value))
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
            }
        }
    }
}