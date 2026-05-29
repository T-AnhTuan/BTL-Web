using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.ViewModels;

namespace QuanLyVatTu.Services
{
    public interface IBaoCaoService
    {
        Task<BaoCaoVM> LayBaoCaoNhapXuatTonAsync(int? khoId, DateTime tuNgay, DateTime denNgay, string tuKhoa);
    }

    public class BaoCaoService : IBaoCaoService
    {
        private readonly AppDbContext _context;

        public BaoCaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaoCaoVM> LayBaoCaoNhapXuatTonAsync(int? khoId, DateTime tuNgay, DateTime denNgay, string tuKhoa)
        {
            // Mở rộng "Đến Ngày" ra hết ngày (23:59:59) để không bỏ sót phiếu
            DateTime denNgayCuoiNgay = denNgay.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // ========================================================
            // 1. LẤY DANH SÁCH VẬT TƯ (Chỉ lọc theo Từ khóa)
            // ========================================================
            var queryVatTu = _context.VatTus.AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                var key = tuKhoa.ToLower();
                queryVatTu = queryVatTu.Where(v => v.MaVatTu.ToLower().Contains(key) || v.TenVatTu.ToLower().Contains(key));
            }

            var danhSachVatTu = await queryVatTu.ToListAsync();

            // ========================================================
            // 2. LẤY GIAO DỊCH NHẬP - XUẤT VÀ LỌC THEO KHO
            // Chỉ lấy các phiếu đã được DUYỆT
            // ========================================================
            var queryNhap = _context.ChiTietPhieuNhaps
                .Include(c => c.PhieuNhap)
                .Where(c => c.PhieuNhap.TrangThai == TrangThaiPhieuNhap.DaDuyet
                && c.PhieuNhap.NgayNhap <= denNgayCuoiNgay);

            var queryXuat = _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat)
                .Where(c => c.PhieuXuat.TrangThai == TrangThaiPhieuXuat.DaDuyet
                 && c.PhieuXuat.NgayXuat <= denNgayCuoiNgay);


            if (khoId.HasValue && khoId.Value > 0)
            {
                queryNhap = queryNhap.Where(c => c.PhieuNhap.KhoId == khoId.Value);
                queryXuat = queryXuat.Where(c => c.PhieuXuat.KhoId == khoId.Value);
            }

            var chiTietNhaps = await queryNhap.ToListAsync();
            var chiTietXuats = await queryXuat.ToListAsync();

            // ========================================================
            // 3. TÍNH TOÁN BÁO CÁO
            // ========================================================
            var viewModel = new BaoCaoVM();

            foreach (var vt in danhSachVatTu)
            {
                var item = new BaoCaoItem
                {
                    VatTuId = vt.Id,
                    MaVatTu = vt.MaVatTu,
                    TenVatTu = vt.TenVatTu,
                    DonViTinh = vt.DonViTinh
                };

                // Tính Tồn Đầu Kỳ (Chỉ lấy trước tuNgay)
                var nhapTruocTuNgay = chiTietNhaps.Where(c => c.VatTuId == vt.Id && c.PhieuNhap.NgayNhap < tuNgay);
                var xuatTruocTuNgay = chiTietXuats.Where(c => c.VatTuId == vt.Id && c.PhieuXuat.NgayXuat < tuNgay);

                item.TonDauSoLuong = nhapTruocTuNgay.Sum(c => c.SoLuong) - xuatTruocTuNgay.Sum(c => c.SoLuong);
                item.TonDauGiaTri = nhapTruocTuNgay.Sum(c => c.ThanhTien) - xuatTruocTuNgay.Sum(c => c.ThanhTien);

                // Tính Nhập Trong Kỳ (Từ tuNgay đến denNgayCuoiNgay)
                var nhapTrongKy = chiTietNhaps.Where(c => c.VatTuId == vt.Id && c.PhieuNhap.NgayNhap >= tuNgay && c.PhieuNhap.NgayNhap <= denNgayCuoiNgay);
                item.NhapSoLuong = nhapTrongKy.Sum(c => c.SoLuong);
                item.NhapGiaTri = nhapTrongKy.Sum(c => c.ThanhTien);

                // Tính Xuất Trong Kỳ
                var xuatTrongKy = chiTietXuats.Where(c => c.VatTuId == vt.Id && c.PhieuXuat.NgayXuat >= tuNgay && c.PhieuXuat.NgayXuat <= denNgayCuoiNgay);
                item.XuatSoLuong = xuatTrongKy.Sum(c => c.SoLuong);
                item.XuatGiaTri = xuatTrongKy.Sum(c => c.ThanhTien);

                // SỬA LỖI Ở ĐÂY: Đã bỏ đi khối lệnh IF chặn vật tư có số liệu = 0.
                // Bây giờ mọi vật tư có trong danh mục đều sẽ được Add vào báo cáo để hiển thị.
                viewModel.DanhSachChiTiet.Add(item);
            }

            return viewModel;
        }
    }
}