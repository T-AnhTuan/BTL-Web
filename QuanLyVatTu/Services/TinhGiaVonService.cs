using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Services
{
    public interface ITinhGiaVonService
    {
        Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId);
    }

    public class TinhGiaVonService : ITinhGiaVonService
    {
        private readonly AppDbContext _context;

        public TinhGiaVonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId)
        {
            try
            {
                var phieuNhap = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.Id == phieuNhapId);

                if (phieuNhap == null || phieuNhap.TrangThai != TrangThaiPhieuNhap.DaDuyet)
                    return false;

                foreach (var chiTiet in phieuNhap.ChiTietPhieuNhaps)
                {
                    var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);
                    if (vatTu != null)
                    {
                        decimal tongGiaTriTonCu = vatTu.TonKhoHienTai * vatTu.GiaVonBinhQuan;
                        decimal tongGiaTriNhapMoi = chiTiet.SoLuong * chiTiet.DonGia;
                        int tongSoLuongMoi = vatTu.TonKhoHienTai + chiTiet.SoLuong;

                        decimal giaVonMoi = 0;
                        if (tongSoLuongMoi > 0)
                        {
                            giaVonMoi = (tongGiaTriTonCu + tongGiaTriNhapMoi) / tongSoLuongMoi;
                        }

                        // Cập nhật Database
                        vatTu.GiaVonBinhQuan = Math.Round(giaVonMoi, 2);
                        vatTu.TonKhoHienTai = tongSoLuongMoi;
                        _context.VatTus.Update(vatTu);

                        var tonKhoTheoKho = await _context.ChiTietKhos
                            .FirstOrDefaultAsync(c => c.KhoId == phieuNhap.KhoId && c.VatTuId == chiTiet.VatTuId);
                        if (tonKhoTheoKho == null)
                        {
                            _context.ChiTietKhos.Add(new ChiTietKho
                            {
                                KhoId = phieuNhap.KhoId,
                                VatTuId = chiTiet.VatTuId,
                                SoLuong = chiTiet.SoLuong
                            });
                        }
                        else
                        {
                            tonKhoTheoKho.SoLuong += chiTiet.SoLuong;
                        }
                    }
                }

                // [MỚI] - GHI NHẬT KÝ HỆ THỐNG ĐÃ TÍNH TOÁN XONG
                _context.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"Hệ thống tự động chạy lại Giá Vốn Bình Quân sau khi nhập phiếu {phieuNhap.MaPhieu}",
                    ThoiGian = DateTime.Now,
                    DiaChiIP = "0.0.0.0"
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // [MỚI] - GHI LOG NẾU LỖI
                _context.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"[LỖI] Tính giá vốn thất bại cho phiếu {phieuNhapId}: {ex.Message}",
                    ThoiGian = DateTime.Now,
                    DiaChiIP = "0.0.0.0"
                });
                await _context.SaveChangesAsync();
                return false;
            }
        }
    }
}
