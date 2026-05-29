// Khai báo sử dụng thư viện System để dùng các tính năng cơ bản của C# (như DateTime, Exception)
using System;
// Khai báo thư viện LINQ để hỗ trợ thao tác, truy vấn với các mảng/danh sách dữ liệu
using System.Linq;
// Khai báo thư viện Task để hỗ trợ viết code chạy bất đồng bộ (Async/Await), giúp web không bị đơ
using System.Threading.Tasks;
// Khai báo Entity Framework Core để tương tác trực tiếp với cơ sở dữ liệu SQL Server
using Microsoft.EntityFrameworkCore;
// Khai báo không gian tên chứa AppDbContext (Nơi cấu hình các bảng CSDL)
using QuanLyVatTu.Data;
// Khai báo không gian tên chứa các Class cấu trúc dữ liệu (Models)
using QuanLyVatTu.Models;

// Đặt toàn bộ code vào trong không gian tên QuanLyVatTu.Services để dễ quản lý
namespace QuanLyVatTu.Services
{
    // Khai báo giao diện (Interface) INhapXuatService đóng vai trò như một "bản hợp đồng"
    public interface INhapXuatService
    {
        // Định nghĩa hàm lập phiếu nhập: Nhận vào 1 tờ Phiếu Nhập, ID tài khoản, trả về True/False kèm Câu thông báo
        Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int taiKhoanId);

        // Định nghĩa hàm duyệt phiếu xuất: Nhận vào ID phiếu xuất. (taiKhoanId = 1 là giá trị mặc định để tránh lỗi Controller)
        Task<(bool IsSuccess, string Message)> DuyetPhieuXuatAsync(int phieuXuatId, int taiKhoanId = 1);
    }

    // Lớp NhapXuatService thực thi (implement) các hàm đã hứa trong INhapXuatService
    public class NhapXuatService : INhapXuatService
    {
        // Khai báo biến _context chỉ đọc (readonly) để chứa đối tượng làm việc với Database
        private readonly AppDbContext _context;

        // Hàm khởi tạo (Constructor): C# sẽ tự động tiêm (inject) AppDbContext vào đây khi ứng dụng chạy
        public NhapXuatService(AppDbContext context)
        {
            // Gán giá trị nhận được vào biến _context để dùng chung cho toàn bộ Class này
            _context = context;
        }

        // ==========================================
        // 1. HÀM LẬP PHIẾU NHẬP MỚI (ĐÃ KHÔI PHỤC)
        // ==========================================
        // Từ khóa async báo hiệu hàm này chạy bất đồng bộ
        public async Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int taiKhoanId)
        {
            // Mở khối try để bẫy lỗi. Nếu code bên trong hỏng, nó sẽ nhảy xuống khối catch
            try
            {
                // Ép trạng thái của Phiếu Nhập mới tạo mặc định luôn là "Chờ Duyệt"
                phieuNhap.TrangThai = TrangThaiPhieuNhap.ChoDuyet;

                // Thêm đối tượng Phiếu Nhập này vào bộ nhớ đệm (tracking) của Entity Framework
                _context.PhieuNhaps.Add(phieuNhap);

                // Tạo một đối tượng Nhật Ký mới để lưu vết lịch sử hệ thống
                var nhatKy = new NhatKyHeThong
                {
                    // Ghi lại ID của người lập phiếu
                    TaiKhoanId = taiKhoanId,
                    // Ghi lại hành động (Chưa có ID phiếu vì chưa lưu nên chỉ ghi chữ chung chung)
                    HanhDong = "Lập phiếu nhập kho mới",
                    // Ghi lại thời gian xảy ra hành động là ngay bây giờ
                    ThoiGian = DateTime.Now,
                    // Ghi lại địa chỉ IP mặc định
                    DiaChiIP = "0.0.0.0"
                };

                // Thêm dòng nhật ký này vào bộ nhớ đệm
                _context.NhatKyHeThongs.Add(nhatKy);

                // Dùng await để chờ SQL Server thực thi việc lưu cả Phiếu Nhập và Nhật Ký xuống Database
                await _context.SaveChangesAsync();

                // Trả về Tuple gồm giá trị true và câu thông báo thành công
                return (true, "Lập phiếu nhập thành công.");
            }
            // Khối catch sẽ hứng lấy biến lỗi (ex) nếu khối try thất bại
            catch (Exception ex)
            {
                // Trả về false và nối kèm nội dung lỗi (ex.Message) để báo ra màn hình
                return (false, $"Lỗi khi lập phiếu: {ex.Message}");
            }
        }

        // ==========================================
        // 2. HÀM DUYỆT PHIẾU XUẤT (VÀ TRỪ TỒN KHO)
        // ==========================================
        // Hàm nhận vào ID của tờ phiếu xuất cần duyệt
        public async Task<(bool IsSuccess, string Message)> DuyetPhieuXuatAsync(int phieuXuatId, int taiKhoanId = 1)
        {
            // Mở khối try để bẫy các lỗi liên quan đến thuật toán trừ kho hoặc SQL
            try
            {
                // Truy vấn tìm tờ phiếu xuất có ID khớp với số truyền vào
                // Gọi Include để "kéo" theo tất cả các dòng chi tiết vật tư của tờ phiếu đó
                var phieu = await _context.PhieuXuats
                    .Include(p => p.ChiTietPhieuXuats)
                    .FirstOrDefaultAsync(p => p.Id == phieuXuatId);

                // Kiểm tra 2 lớp bảo vệ: Phiếu phải tồn tại (khác null) và trạng thái không được là Đã Duyệt
                if (phieu == null || phieu.TrangThai == TrangThaiPhieuXuat.DaDuyet)
                {
                    // Bị chặn lại, trả về false và lý do
                    return (false, "Phiếu không tồn tại hoặc đã được duyệt từ trước.");
                }

                // Vòng lặp foreach duyệt qua từng dòng hàng hóa có trong phiếu xuất
                foreach (var chiTiet in phieu.ChiTietPhieuXuats)
                {
                    // Chạy xuống SQL tìm món vật tư đang xét dựa vào Mã vật tư (VatTuId)
                    var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);

                    // Nếu tìm thấy vật tư đó trong danh mục
                    if (vatTu != null)
                    {
                        // Kiểm tra kho: Nếu số lượng định xuất lớn hơn số lượng tồn kho hiện tại
                        if (chiTiet.SoLuong > vatTu.TonKhoHienTai)
                        {
                            // Chủ động ném ra (throw) một ngoại lệ để ngay lập tức dừng hàm, chặn xuất âm kho
                            throw new Exception($"Vật tư {vatTu.TenVatTu} không đủ tồn kho (Tồn: {vatTu.TonKhoHienTai}).");
                        }

                        // Thực hiện trừ tồn kho: Lấy số tồn hiện tại trừ đi số lượng xuất
                        vatTu.TonKhoHienTai -= chiTiet.SoLuong;

                        // Lấy giá vốn của vật tư hiện tại gán ngược lại làm Đơn Giá cho dòng xuất này
                        chiTiet.DonGiaXuat = vatTu.GiaVonBinhQuan;

                        // CẢNH BÁO TỒN KHO: Kiểm tra nếu trừ xong mà tồn kho rớt xuống <= 10
                        if (vatTu.TonKhoHienTai <= 10)
                        {
                            // Tạo một đối tượng Thông báo hệ thống mới
                            var thongBao = new ThongBao
                            {
                                // Gán tiêu đề có chứa icon cảnh báo
                                TieuDe = "⚠️ Cảnh báo tồn kho thấp",
                                // Ghi rõ tên vật tư và số lượng còn lại để nhắc nhở
                                NoiDung = $"Vật tư [{vatTu.MaVatTu}] - {vatTu.TenVatTu} chỉ còn {vatTu.TonKhoHienTai} {vatTu.DonViTinh}. Vui lòng nhập thêm!",
                                // Gửi cảnh báo này cho tài khoản Quản trị (ID = 1)
                                TaiKhoanId = 1,
                                // Ghi chú thời gian sinh ra cảnh báo
                                NgayTao = DateTime.Now,
                                // Đặt cờ DaXem thành false (báo hiệu là chưa đọc)
                                DaXem = false
                            };
                            // Gắn thông báo này vào hàng đợi để lưu
                            _context.ThongBaos.Add(thongBao);
                        }
                    }
                }

                // Sau khi trừ kho xong xuôi, đổi cờ trạng thái của Phiếu Xuất sang "Đã Duyệt"
                phieu.TrangThai = TrangThaiPhieuXuat.DaDuyet;

                // Đánh dấu cho EF Core biết là tờ phiếu này đã bị sửa đổi
                _context.PhieuXuats.Update(phieu);

                // Tạo đối tượng Nhật Ký Hệ Thống để lưu vết duyệt phiếu
                var nhatKy = new NhatKyHeThong
                {
                    // Lấy ID người bấm duyệt
                    TaiKhoanId = taiKhoanId,
                    // Lưu câu mô tả hành động kèm Mã phiếu (Dùng D5 để tạo số 0000X)
                    HanhDong = $"Phê duyệt Phiếu xuất kho: PX-{phieu.Id.ToString("D5")}",
                    // Thời gian bấm duyệt
                    ThoiGian = DateTime.Now,
                    // Ghi IP mặc định
                    DiaChiIP = "0.0.0.0"
                };
                // Đưa nhật ký vào hàng đợi
                _context.NhatKyHeThongs.Add(nhatKy);

                // MỆNH LỆNH QUAN TRỌNG NHẤT: Chờ SQL Server thực thi tất cả các hành động trên (Update kho, Cập nhật phiếu, Lưu log)
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công hoàn toàn
                return (true, "Duyệt phiếu xuất và trừ tồn kho thành công.");
            }
            // Khối bẫy lỗi sẽ bắt trúng cái `throw new Exception` (Lỗi thiếu kho) ở trên hoặc các lỗi SQL
            catch (Exception ex)
            {
                // Trả về false và báo cáo nguyên nhân thất bại
                return (false, $"Lỗi duyệt phiếu: {ex.Message}");
            }
        }
    }
}