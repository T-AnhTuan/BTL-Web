using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using QuanLyVatTu.ViewModels; // Bắt buộc phải có để gọi ViewModel báo cáo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Services
{
    // ==========================================
    // 1. INTERFACE
    // ==========================================
    public interface IBaoCaoService
    {
        // Nhóm hàm phục vụ Báo Cáo Kế Toán
        Task<BaoCaoVM> LayBaoCaoNhapXuatTonAsync(int? khoId, DateTime tuNgay, DateTime denNgay, string tuKhoa);

        // Nhóm hàm phục vụ Dashboard / Header
        Task<List<ThongBao>> LayThongBaoChuaXemAsync(int taiKhoanId);
        Task<List<NhatKyHeThong>> LayNhatKyGiaoDichMoiNhatAsync(int soLuong = 10);
    }

    // ==========================================
    // 2. CLASS IMPLEMENTATION
    // ==========================================
    public class BaoCaoService : IBaoCaoService
    {
        private readonly AppDbContext _context;

        public BaoCaoService(AppDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // HÀM 1: LẤY THÔNG BÁO CHO HEADER CỦA USER
        // --------------------------------------------------
        public async Task<List<ThongBao>> LayThongBaoChuaXemAsync(int taiKhoanId)
        {
            return await _context.ThongBaos
                .Where(t => t.TaiKhoanId == taiKhoanId && t.DaXem == false)
                .OrderByDescending(t => t.NgayTao)
                .Take(5) // Chỉ lấy 5 thông báo mới nhất
                .ToListAsync();
        }

        // --------------------------------------------------
        // HÀM 2: LẤY NHẬT KÝ CHO DASHBOARD
        // --------------------------------------------------
        public async Task<List<NhatKyHeThong>> LayNhatKyGiaoDichMoiNhatAsync(int soLuong = 10)
        {
            return await _context.NhatKyHeThongs
                .Include(n => n.TaiKhoan)
                    .ThenInclude(t => t.NhanVien) // Lấy ra tên nhân viên
                .OrderByDescending(n => n.ThoiGian)
                .Take(soLuong)
                .ToListAsync();
        }

        // --------------------------------------------------
        // HÀM 3: TÍNH TOÁN BÁO CÁO XUẤT - NHẬP - TỒN
        // --------------------------------------------------
        public async Task<BaoCaoVM> LayBaoCaoNhapXuatTonAsync(int? khoId, DateTime tuNgay, DateTime denNgay, string tuKhoa)
        {
            var vm = new BaoCaoVM
            {
                KhoId = khoId,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                TuKhoa = tuKhoa
            };

            // Bước 1: Lấy danh sách vật tư theo từ khóa tìm kiếm
            var queryVatTu = _context.VatTus.AsQueryable();
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                tuKhoa = tuKhoa.ToLower();
                queryVatTu = queryVatTu.Where(v => v.MaVatTu.ToLower().Contains(tuKhoa) || v.TenVatTu.ToLower().Contains(tuKhoa));
            }

            var danhSachVatTu = await queryVatTu.ToListAsync();

            // Bước 2: Lấy dữ liệu Nhập - Xuất trong khoảng thời gian đã được DUYỆT
            var chiTietNhaps = await _context.ChiTietPhieuNhaps
                .Include(c => c.PhieuNhap)
                .Where(c => c.PhieuNhap.TrangThai == TrangThaiPhieuNhap.DaDuyet)
                .ToListAsync();

            var chiTietXuats = await _context.ChiTietPhieuXuats
                .Include(c => c.PhieuXuat)
                .Where(c => c.PhieuXuat.TrangThai == TrangThaiPhieuXuat.DaDuyet)
                .ToListAsync();

            // Bước 3: Thuật toán cộng trừ tồn kho cho từng mã vật tư
            foreach (var vt in danhSachVatTu)
            {
                var item = new BaoCaoItem
                {
                    VatTuId = vt.Id,
                    MaVatTu = vt.MaVatTu,
                    TenVatTu = vt.TenVatTu,
                    DonViTinh = vt.DonViTinh
                };

                // Tính Nhập trong kỳ
                var nhapTrongKy = chiTietNhaps.Where(c => c.VatTuId == vt.Id && c.PhieuNhap.NgayNhap >= tuNgay && c.PhieuNhap.NgayNhap <= denNgay);
                item.NhapSoLuong = nhapTrongKy.Sum(c => c.SoLuong);
                item.NhapGiaTri = nhapTrongKy.Sum(c => c.ThanhTien);

                // Tính Xuất trong kỳ
                var xuatTrongKy = chiTietXuats.Where(c => c.VatTuId == vt.Id && c.PhieuXuat.NgayXuat >= tuNgay && c.PhieuXuat.NgayXuat <= denNgay);
                item.XuatSoLuong = xuatTrongKy.Sum(c => c.SoLuong);
                item.XuatGiaTri = xuatTrongKy.Sum(c => c.ThanhTien);

                // Tính Tồn đầu kỳ (Truy hồi từ Tồn cuối thực tế trừ đi phát sinh trong kỳ)
                item.TonDauSoLuong = vt.TonKhoHienTai - item.NhapSoLuong + item.XuatSoLuong;
                item.TonDauGiaTri = item.TonDauSoLuong * vt.GiaVonBinhQuan;

                vm.DanhSachChiTiet.Add(item);
            }

            return vm;
        }
    }
}