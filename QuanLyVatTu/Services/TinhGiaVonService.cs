using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Services
{
    public interface ITinhGiaVonService
    {
        Task<bool> TruTonKhoSauKhiXuatAsync(int phieuXuatId, int taiKhoanId);
        Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId);
    }

    public class TinhGiaVonService : ITinhGiaVonService
    {
        private readonly AppDbContext _context;
        private readonly INhatKyService _nhatKyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TinhGiaVonService(IHttpContextAccessor httpContextAccessor,AppDbContext context, INhatKyService nhatKyService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _nhatKyService = nhatKyService;
        }

        public async Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var phieuNhap = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.Id == phieuNhapId);

                if (phieuNhap == null || phieuNhap.TrangThai != TrangThaiPhieuNhap.DaDuyet)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Vòng lặp duyệt qua từng món hàng (chi tiết) có trong phiếu nhập
                foreach (var chiTiet in phieuNhap.ChiTietPhieuNhaps)
                {

                    var vatTu = await _context.VatTus
                        .FindAsync(chiTiet.VatTuId);
                    if (vatTu != null)
                    {
                        if (!CheckOverflowRisk(vatTu.TonKhoHienTai, vatTu.GiaVonBinhQuan))
                        {
                            throw new InvalidOperationException(
                                $"Dữ liệu quá lớn - Vật tư {vatTu.MaVatTu}: " +
                                $"Tồn kho ({vatTu.TonKhoHienTai}) × Giá ({vatTu.GiaVonBinhQuan}) vượt quá giới hạn");
                        }
                       
                        if (!CheckOverflowRisk(chiTiet.SoLuong, chiTiet.DonGia))
                        {
                            throw new InvalidOperationException(
                                $"Dữ liệu quá lớn - Chi tiết: Số lượng ({chiTiet.SoLuong}) × Đơn giá ({chiTiet.DonGia}) vượt quá giới hạn");
                        }
                        // BƯỚC 2: TÍNH TỔNG GIÁ TRỊ LÔ HÀNG VỪA NHẬP
                        decimal currentQty = vatTu.TonKhoHienTai;
                        decimal incomingQty = chiTiet.SoLuong;
                        decimal previousQty = currentQty - incomingQty;
                        if (previousQty < 0) previousQty = 0m;

                        decimal tongGiaTriTonCu = vatTu.TonKhoHienTai * vatTu.GiaVonBinhQuan;
                        decimal tongGiaTriNhapMoi = chiTiet.SoLuong * chiTiet.DonGia;

                        // BƯỚC 3: CỘNG DỒN SỐ LƯỢNG TỒN KHO (Nhập vào thì tăng số lượng)
                        int tonKhoMoi = vatTu.TonKhoHienTai + chiTiet.SoLuong;

                        // BƯỚC 4: TÍNH GIÁ VÓN BÍNH QUẢN
                        if (tonKhoMoi > 0)
                        {
                            vatTu.GiaVonBinhQuan = (tongGiaTriTonCu + tongGiaTriNhapMoi) / tonKhoMoi;
                            if (vatTu.GiaVonBinhQuan < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Giá vốn bình quân âm (không hợp lệ) cho vật tư {vatTu.MaVatTu}: {vatTu.GiaVonBinhQuan}");
                            }
                            vatTu.TonKhoHienTai = tonKhoMoi; 
                            _context.VatTus.Update(vatTu);
                        }
                        var entry = new NhatKyHeThong
                        {
                            TaiKhoanId = taiKhoanId,
                            HanhDong = $"Hệ thống tự động cập nhật Giá Vốn Bình Quân sau khi nhập phiếu PN-{phieuNhap.Id.ToString("D5")}",
                            DiaChiIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            ThoiGian = DateTime.Now
                        };
                        await _nhatKyService.GhiNhatKyAsync(entry);
                       
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { /* ignore */ }
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"[LỖI] Tính giá vốn thất bại cho phiếu {phieuNhapId}: {ex.Message}",
                    DiaChiIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                await _context.SaveChangesAsync();
                return false;
            }
        }
        public async Task<bool> TruTonKhoSauKhiXuatAsync(int phieuXuatId, int taiKhoanId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieuXuat = await _context.PhieuXuats
                    .Include(p => p.ChiTietPhieuXuats)
                    .FirstOrDefaultAsync(p => p.Id == phieuXuatId);

                if (phieuXuat == null || phieuXuat.TrangThai != TrangThaiPhieuXuat.DaDuyet)
                {
                    return false;
                }

                foreach (var chiTiet in phieuXuat.ChiTietPhieuXuats)
                {
                    var vatTu = await _context.VatTus.FirstOrDefaultAsync(v => v.Id == chiTiet.VatTuId);

                    if (vatTu != null)
                    {
                        chiTiet.DonGiaXuat = vatTu.GiaVonBinhQuan; 
                        // TRỪ TỒN KHO (Khi xuất hàng ra)
                        vatTu.TonKhoHienTai -= chiTiet.SoLuong;

                        // Đảm bảo tồn kho không bị âm vô lý
                        if (vatTu.TonKhoHienTai < 0)
                            vatTu.TonKhoHienTai = 0;

                        _context.VatTus.Update(vatTu);
                        _context.ChiTietPhieuXuats.Update(chiTiet);
                    }
                }

                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"Hệ thống tự động trừ tôn kho sau khi xuat phiếu PX-{phieuXuat.Id.ToString("D5")}",
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                await transaction.CommitAsync();
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"[LỖI] Trừ tồn kho thất bại cho phiếu xuất {phieuXuatId}: {ex.Message}",
                    ThoiGian = DateTime.Now
                };
               try { await transaction.RollbackAsync(); } catch { /* ignore */ }
                await _nhatKyService.GhiNhatKyAsync(entry);
                await _context.SaveChangesAsync();
                return false;
            }
        }
        private bool CheckOverflowRisk(long quantity, decimal price)
        {
            try
            {
                // Tính toán với kiểm tra overflow
                // Nếu quantity hoặc price quá lớn, sẽ ném ra OverflowException
                checked
                {
                    var result = (decimal)quantity * price;
                }
                return true;
            }
            catch (OverflowException)
            {
                // Nếu có overflow, trả về false
                return false;
            }
        }
    }
}