// Khai báo sử dụng thư viện System
using System;
// Khai báo sử dụng LINQ để duyệt mảng
using System.Linq;
// Khai báo sử dụng bất đồng bộ Task
using System.Threading.Tasks;
// Khai báo thư viện tương tác CSDL Entity Framework Core
using Microsoft.EntityFrameworkCore;
// Khai báo namespace chứa file cấu hình Database
using QuanLyVatTu.Data;
// Khai báo namespace chứa các Class cấu trúc dữ liệu
using QuanLyVatTu.Models;

// Bắt đầu không gian tên của Services
namespace QuanLyVatTu.Services
{
    // Tạo Interface (hợp đồng) chứa hàm Tính giá vốn
    public interface ITinhGiaVonService
    {
        // Định nghĩa hàm Tính giá vốn nhận vào 2 tham số: Mã phiếu nhập và Mã người thực hiện
        Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId);
    }

    // Class thực thi logic tính toán giá vốn
    public class TinhGiaVonService : ITinhGiaVonService
    {
        // Biến toàn cục chứa kết nối DB
        private readonly AppDbContext _context;

        // Constructor tiêm (inject) database vào
        public TinhGiaVonService(AppDbContext context)
        {
            // Gán kết nối DB vào biến toàn cục
            _context = context;
        }

        // Bắt đầu viết logic tính giá vốn sau khi nhập
        public async Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId, int taiKhoanId)
        {
            try // Bọc trong khối try-catch để bắt lỗi nếu có sự cố tính toán
            {
                // Tìm phiếu nhập gốc dựa vào ID truyền vào
                // Kéo theo (Include) danh sách chi tiết vật tư của phiếu đó
                var phieuNhap = await _context.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.Id == phieuNhapId);

                // Kiểm tra an toàn: Bỏ qua nếu phiếu không tồn tại hoặc chưa được duyệt
                // SỬA LỖI: Dùng TrangThaiPhieu.DaDuyet thay cho TrangThaiPhieuNhap
                if (phieuNhap == null || phieuNhap.TrangThai != TrangThaiPhieuNhap.DaDuyet)
                {
                    // Trả về false vì không đủ điều kiện tính toán
                    return false;
                }

                // Vòng lặp duyệt qua từng món hàng (chi tiết) có trong phiếu nhập
                foreach (var chiTiet in phieuNhap.ChiTietPhieuNhaps)
                {
                    // Tìm món vật tư tương ứng trong DB bằng ID
                    var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);

                    // Nếu vật tư tồn tại
                    if (vatTu != null)
                    {
                        // BƯỚC 1: TÍNH TỔNG GIÁ TRỊ TỒN KHO CŨ
                        // Số lượng đang có nhân với Giá vốn hiện tại (Nếu rỗng thì tính là 0)
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