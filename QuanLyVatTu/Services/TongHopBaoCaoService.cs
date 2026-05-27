using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Services
{
    // =========================================================================
    // 1. ĐỊNH NGHĨA CÁC LỚP CHỨA DỮ LIỆU (DTO - Data Transfer Object)
    // Dùng để đóng gói kết quả từ Service gửi sang Controller
    // =========================================================================

    public class BaoCaoTonKhoDto
    {
        public List<VatTu> DanhSachVatTu { get; set; } = new List<VatTu>();
        public int TongSoLoaiVatTu { get; set; }
        public decimal TongGiaTriTonKho { get; set; }
        public int CanhBaoHetHang { get; set; }
    }

    public class BaoCaoNhapXuatDto
    {
        public List<PhieuNhap> DanhSachNhap { get; set; } = new List<PhieuNhap>();
        public List<PhieuXuat> DanhSachXuat { get; set; } = new List<PhieuXuat>();
        public decimal TongTienNhap { get; set; }
        public decimal TongTienXuat { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
    }

    public class ThongKeDto
    {
        public string[] ChartLabels { get; set; } // Nhãn biểu đồ (VD: Tên các nhóm vật tư)
        public decimal[] ChartData { get; set; } // Số liệu biểu đồ (VD: Giá trị tồn kho)
        public decimal TyLeLuanChuyen { get; set; } // Hiệu suất kho
        public string DanhGiaHieuSuat { get; set; }
    }

    // =========================================================================
    // 2. TẠO INTERFACE CHO SERVICE (Dùng cho Dependency Injection)
    // =========================================================================
    public interface ITongHopBaoCaoService
    {
        Task<BaoCaoTonKhoDto> LayDuLieuBaoCaoTonKhoAsync();
        Task<BaoCaoNhapXuatDto> LayDuLieuBaoCaoNhapXuatAsync(DateTime? tuNgay, DateTime? denNgay);
        Task<ThongKeDto> LayDuLieuPhanTichThongKeAsync();
    }

    // =========================================================================
    // 3. TRIỂN KHAI LOGIC CHI TIẾT
    // =========================================================================
    public class TongHopBaoCaoService : ITongHopBaoCaoService
    {
        private readonly AppDbContext _context;

        public TongHopBaoCaoService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Xử lý logic cho Báo cáo tồn kho
        /// </summary>
        public async Task<BaoCaoTonKhoDto> LayDuLieuBaoCaoTonKhoAsync()
        {
            var danhSach = await _context.VatTus.OrderBy(v => v.TenVatTu).ToListAsync();

            return new BaoCaoTonKhoDto
            {
                DanhSachVatTu = danhSach,
                TongSoLoaiVatTu = danhSach.Count,
                TongGiaTriTonKho = danhSach.Sum(v => v.SoLuongTon * v.GiaVonTB),
                CanhBaoHetHang = danhSach.Count(v => v.SoLuongTon <= 10) // Mức cảnh báo giả định là 10
            };
        }

        /// <summary>
        /// Xử lý logic cho Báo cáo Nhập Xuất (Có lọc theo thời gian)
        /// </summary>
        public async Task<BaoCaoNhapXuatDto> LayDuLieuBaoCaoNhapXuatAsync(DateTime? tuNgay, DateTime? denNgay)
        {
            // Thiết lập giá trị mặc định nếu người dùng không chọn ngày (lấy tháng hiện tại)
            DateTime startDate = tuNgay ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = denNgay ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

            // Truy vấn Phiếu Nhập
            var phieuNhaps = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .Where(p => p.NgayNhap >= startDate && p.NgayNhap <= endDate)
                .ToListAsync();

            // Truy vấn Phiếu Xuất (Chỉ lấy phiếu Đã duyệt)
            var phieuXuats = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                .Where(p => p.NgayXuat >= startDate && p.NgayXuat <= endDate && p.TrangThai == "Đã duyệt")
                .ToListAsync();

            return new BaoCaoNhapXuatDto
            {
                DanhSachNhap = phieuNhaps,
                DanhSachXuat = phieuXuats,
                TongTienNhap = phieuNhaps.Sum(p => p.ChiTietPhieuNhaps.Sum(ct => ct.SoLuongThucNhap * ct.DonGia)),
                TongTienXuat = phieuXuats.Sum(p => p.ChiTietPhieuXuats.Sum(ct => ct.SoLuongThucXuat * ct.DonGia)),
                TuNgay = startDate,
                DenNgay = endDate
            };
        }

        /// <summary>
        /// Xử lý logic phức tạp để vẽ Biểu đồ và tính Hiệu suất kho
        /// </summary>
        public async Task<ThongKeDto> LayDuLieuPhanTichThongKeAsync()
        {
            // 1. Gom nhóm dữ liệu cho Biểu đồ (Nhóm theo Đơn vị tính hoặc Danh mục)
            var coCauTonKho = await _context.VatTus
                .GroupBy(v => v.DonViTinh)
                .Select(g => new
                {
                    TenNhom = g.Key,
                    TongGiaTri = g.Sum(v => v.SoLuongTon * v.GiaVonTB)
                })
                .ToListAsync();

            // 2. Tính Tỷ lệ luân chuyển (Hiệu suất) = (Tổng vốn xuất trong năm) / (Giá trị tồn kho hiện tại)
            var tongGiaTriTon = await _context.VatTus.SumAsync(v => v.SoLuongTon * v.GiaVonTB);

            var tongXuatNamNay = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                .Where(p => p.NgayXuat.Year == DateTime.Now.Year && p.TrangThai == "Đã duyệt")
                .SumAsync(p => p.ChiTietPhieuXuats.Sum(ct => ct.SoLuongThucXuat * ct.DonGia));

            decimal tyLe = tongGiaTriTon > 0 ? (tongXuatNamNay / tongGiaTriTon) : 0;

            return new ThongKeDto
            {
                ChartLabels = coCauTonKho.Select(c => c.TenNhom).ToArray(),
                ChartData = coCauTonKho.Select(c => c.TongGiaTri).ToArray(),
                TyLeLuanChuyen = Math.Round(tyLe, 2),
                DanhGiaHieuSuat = tyLe > 2 ? "Tốt (Hàng lưu thông nhanh)" : "Chậm (Nguy cơ tồn đọng)"
            };
        }
    }
}