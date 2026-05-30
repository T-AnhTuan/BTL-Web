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
            try
            {

                var phieuNhap = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.Id == phieuNhapId);

                if (phieuNhap == null || phieuNhap.TrangThai != TrangThaiPhieuNhap.DaDuyet)
                {
                    return false;
                }

                // Vòng lặp duyệt qua từng món hàng (chi tiết) có trong phiếu nhập
                foreach (var chiTiet in phieuNhap.ChiTietPhieuNhaps)
                {

                    var vatTu = await _context.VatTus
                        .FindAsync(chiTiet.VatTuId);
                    if (vatTu == null)
                    {
                        if (!CheckOverflowRisk(vatTu.TonKhoHienTai, vatTu.GiaVonBinhQuan))
                        {
                            throw new InvalidOperationException(
                                $"Dữ liệu quá lớn - Vật tư {vatTu.MaVatTu}: " +
                                $"Tồn kho ({vatTu.TonKhoHienTai}) × Giá ({vatTu.GiaVonBinhQuan}) vượt quá giới hạn");
                        }
                        decimal tongGiaTriTonCu = (decimal)vatTu.TonKhoHienTai * vatTu.GiaVonBinhQuan;
                        if (!CheckOverflowRisk(chiTiet.SoLuong, chiTiet.DonGia))
                        {
                            throw new InvalidOperationException(
                                $"Dữ liệu quá lớn - Chi tiết: Số lượng ({chiTiet.SoLuong}) × Đơn giá ({chiTiet.DonGia}) vượt quá giới hạn");
                        }
                        // BƯỚC 2: TÍNH TỔNG GIÁ TRỊ LÔ HÀNG VỪA NHẬP
                        decimal tongGiaTriNhapMoi = (decimal)chiTiet.SoLuong * chiTiet.DonGia;

                        // BƯỚC 3: CỘNG DỒN SỐ LƯỢNG TỒN KHO (Nhập vào thì tăng số lượng)
                        vatTu.TonKhoHienTai += chiTiet.SoLuong;

                        // BƯỚC 4: TÍNH GIÁ VÓN BÍNH QUẢN
                        if (vatTu.TonKhoHienTai > 0)
                        {
                            vatTu.GiaVonBinhQuan = (tongGiaTriTonCu + tongGiaTriNhapMoi) / (decimal)vatTu.TonKhoHienTai;
                            if (vatTu.GiaVonBinhQuan < 0)
                            {
                                throw new InvalidOperationException(
                                    $"Giá vốn bình quân âm (không hợp lệ) cho vật tư {vatTu.MaVatTu}: {vatTu.GiaVonBinhQuan}");
                            }
                        }
                        else
                        {
                            vatTu.GiaVonBinhQuan = 0;
                        }

                        // Đánh dấu vật tư này đã bị thay đổi để EF Core lưu lại
                        _context.VatTus.Update(vatTu);
                    }
                }
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"Hệ thống tự động cập nhật Giá Vốn Bình Quân sau khi nhập phiếu PN-{phieuNhap.Id.ToString("D5")}",
                    DiaChiIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                // Cuối cùng, thực thi lệnh lưu toàn bộ quá trình tính toán và nhật ký xuống SQL
                await _context.SaveChangesAsync();

                // Báo cáo hàm chạy thành công 100%
                return true;
            }
            // Khối Catch sẽ bắt mọi lỗi xảy ra trong khối Try ở trên
            catch (Exception ex)
            {
                var entry = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = $"[LỖI] Tính giá vốn thất bại cho phiếu {phieuNhapId}: {ex.Message}",
                    DiaChiIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                };
                await _nhatKyService.GhiNhatKyAsync(entry);
                // Lưu log lỗi xuống SQL
                await _context.SaveChangesAsync();

                // Báo cáo hàm chạy thất bại
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