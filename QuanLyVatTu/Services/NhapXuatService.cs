using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Services
{
    public interface INhapXuatService
    {
        Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int nguoiDungId);
        Task<(bool IsSuccess, string Message)> PheDuyetPhieuXuatAsync(int phieuXuatId, int nguoiDungId);
    }

    public class NhapXuatService : INhapXuatService
    {
        private readonly AppDbContext _context;

        public NhapXuatService(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LẬP PHIẾU NHẬP MỚI
        // ==========================================
        public async Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int taiKhoanId)
        {
            try
            {
                phieuNhap.TrangThai = TrangThaiPhieuNhap.ChoDuyet;
                _context.PhieuNhaps.Add(phieuNhap);

                // [MỚI] - GHI NHẬT KÝ HỆ THỐNG
                _context.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"Đã lập Phiếu nhập mới: {phieuNhap.MaPhieu}",
                    ThoiGian = DateTime.Now,
                    DiaChiIP = "0.0.0.0"
                });

                await _context.SaveChangesAsync();
                return (true, "Lập phiếu nhập và ghi nhật ký thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ==========================================
        // 2. DUYỆT PHIẾU XUẤT (Trừ tồn kho & Cảnh báo)
        // ==========================================
        public async Task<(bool IsSuccess, string Message)> PheDuyetPhieuXuatAsync(int phieuXuatId, int taiKhoanId)
        {
            var phieu = await _context.PhieuXuats
                .Include(p => p.ChiTietPhieuXuats)
                .FirstOrDefaultAsync(p => p.Id == phieuXuatId);

            if (phieu == null || phieu.TrangThai == TrangThaiPhieuXuat.DaDuyet)
                return (false, "Phiếu không tồn tại hoặc đã được duyệt rồi.");

            if (phieu.ChiTietPhieuXuats == null || !phieu.ChiTietPhieuXuats.Any())
                return (false, "Phiếu xuất chưa có vật tư, không thể duyệt.");

            foreach (var chiTiet in phieu.ChiTietPhieuXuats)
            {
                var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);
                if (vatTu != null)
                {
                    if (vatTu.TonKhoHienTai < chiTiet.SoLuong)
                        return (false, $"Lỗi: Vật tư {vatTu.TenVatTu} không đủ để xuất!");

                    // Trừ tồn kho thực tế
                    vatTu.TonKhoHienTai -= chiTiet.SoLuong;
                    _context.VatTus.Update(vatTu);

                    var tonKhoTheoKho = await _context.ChiTietKhos
                        .FirstOrDefaultAsync(c => c.KhoId == phieu.KhoId && c.VatTuId == chiTiet.VatTuId);
                    if (tonKhoTheoKho != null)
                    {
                        if (tonKhoTheoKho.SoLuong < chiTiet.SoLuong)
                            return (false, $"Lỗi: Vật tư {vatTu.TenVatTu} không đủ tồn trong kho xuất!");

                        tonKhoTheoKho.SoLuong -= chiTiet.SoLuong;
                    }

                    // [MỚI] - TẠO THÔNG BÁO NẾU TỒN KHO RỚT XUỐNG MỨC THẤP (< 10)
                    if (vatTu.TonKhoHienTai <= 10)
                    {
                        _context.ThongBaos.Add(new ThongBao
                        {
                            TieuDe = "⚠️ Cảnh báo tồn kho thấp",
                            NoiDung = $"Vật tư [{vatTu.MaVatTu}] - {vatTu.TenVatTu} chỉ còn {vatTu.TonKhoHienTai} {vatTu.DonViTinh}. Vui lòng nhập thêm hàng!",
                            TaiKhoanId = 1, // Giả sử 1 là ID của Admin/Quản lý kho
                            NgayTao = DateTime.Now,
                            DaXem = false
                        });
                    }
                }
            }

            // Cập nhật trạng thái phiếu
            phieu.TrangThai = TrangThaiPhieuXuat.DaDuyet;
            _context.PhieuXuats.Update(phieu);

            // [MỚI] - GHI NHẬT KÝ
            _context.NhatKyHeThongs.Add(new NhatKyHeThong
            {
                TaiKhoanId = taiKhoanId,
                HanhDong = $"Phê duyệt Phiếu xuất kho: {phieu.MaPhieu}",
                ThoiGian = DateTime.Now,
                DiaChiIP = "0.0.0.0"
            });

            await _context.SaveChangesAsync();
            return (true, "Đã phê duyệt phiếu xuất, trừ tồn kho và tạo cảnh báo (nếu có) thành công!");
        }
    }
}
