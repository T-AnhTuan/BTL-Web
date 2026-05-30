
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
                {

                    return false;
                }

                // Vòng lặp duyệt qua từng món hàng (chi tiết) có trong phiếu nhập
                foreach (var chiTiet in phieuNhap.ChiTietPhieuNhaps)
                {

                    var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);

                    if (vatTu != null)
                    {

                        decimal tongGiaTriTonCu = (decimal)vatTu.TonKhoHienTai * vatTu.GiaVonBinhQuan;

                        // BƯỚC 2: TÍNH TỔNG GIÁ TRỊ LÔ HÀNG VỪA NHẬP
                        // Lấy Số lượng nhập nhân với Đơn giá nhập của dòng chi tiết đó
                        decimal tongGiaTriNhapMoi = (decimal)chiTiet.SoLuong * chiTiet.DonGia;

                        // BƯỚC 3: CỘNG DỒN SỐ LƯỢNG TỒN KHO (Nhập vào thì tăng số lượng)
                        vatTu.TonKhoHienTai += chiTiet.SoLuong;

                        // BƯỚC 4: TÍNH LẠI GIÁ VỐN MỚI (BÌNH QUÂN GIA QUYỀN)
                        // Công thức = (Tổng tiền cũ + Tổng tiền mới) / Tổng số lượng sau khi nhập
                        // Kiểm tra nếu Tồn kho > 0 để tránh lỗi chia cho số 0 (DivideByZeroException)
                        if (vatTu.TonKhoHienTai > 0)
                        {
                            // Thực hiện phép chia để ra Giá vốn mới nhất
                            vatTu.GiaVonBinhQuan = (tongGiaTriTonCu + tongGiaTriNhapMoi) / (decimal)vatTu.TonKhoHienTai;
                        }

                        // Đánh dấu vật tư này đã bị thay đổi để EF Core lưu lại
                        _context.VatTus.Update(vatTu);
                    }
                }

                // TẠO NHẬT KÝ HỆ THỐNG để lưu lại lịch sử tính toán
                var nhatKy = new NhatKyHeThong
                {
                    // Lưu ID tài khoản thực hiện
                    TaiKhoanId = taiKhoanId,
                    // Lưu chuỗi mô tả thao tác, SỬA LỖI: Dùng phieuNhap.Id thay vì MaPhieu
                    HanhDong = $"Hệ thống tự động cập nhật Giá Vốn Bình Quân sau khi nhập phiếu PN-{phieuNhap.Id.ToString("D5")}",
                    // Ghi lại mốc thời gian
                    ThoiGian = DateTime.Now,
                    // Địa chỉ IP mặc định
                    DiaChiIP = "0.0.0.0"
                };
                // Thêm dòng nhật ký vào Database
                _context.NhatKyHeThongs.Add(nhatKy);

                // Cuối cùng, thực thi lệnh lưu toàn bộ quá trình tính toán và nhật ký xuống SQL
                await _context.SaveChangesAsync();

                // Báo cáo hàm chạy thành công 100%
                return true;
            }
            // Khối Catch sẽ bắt mọi lỗi xảy ra trong khối Try ở trên
            catch (Exception ex)
            {
                // Nếu bị lỗi tính toán, hệ thống sẽ tự động ghi 1 dòng LOG LỖI vào Database để Dev kiểm tra
                var logLoi = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    // In ra nội dung lỗi cụ thể (ex.Message)
                    HanhDong = $"[LỖI] Tính giá vốn thất bại cho phiếu {phieuNhapId}: {ex.Message}",
                    ThoiGian = DateTime.Now,
                    DiaChiIP = "0.0.0.0"
                };
                // Đẩy log lỗi vào DB
                _context.NhatKyHeThongs.Add(logLoi);
                // Lưu log lỗi xuống SQL
                await _context.SaveChangesAsync();

                // Báo cáo hàm chạy thất bại
                return false;
            }
        }
    }
}