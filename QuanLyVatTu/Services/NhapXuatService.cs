using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
namespace QuanLyVatTu.Services
{
    public interface INhapXuatService
    {
        Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int taiKhoanId);
        Task<(bool IsSuccess, string Message)> DuyetPhieuXuatAsync(int phieuXuatId, int taiKhoanId = 1);
    }

    public class NhapXuatService : INhapXuatService
    {
        private readonly AppDbContext _context; private readonly IWebHostEnvironment _webHostEnvironment;
        public NhapXuatService(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. HÀM LẬP PHIẾU NHẬP MỚI 
        // ==========================================
        public async Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, int taiKhoanId)
        {
            // Mở khối try để bẫy lỗi. Nếu code bên trong hỏng, nó sẽ nhảy xuống khối catch
            try
            {
                phieuNhap.TrangThai = TrangThaiPhieuNhap.ChoDuyet;

                _context.PhieuNhaps.Add(phieuNhap);

                var nhatKy = new NhatKyHeThong
                {
                    TaiKhoanId = taiKhoanId,
                    HanhDong = "Lập phiếu nhập kho mới",
                    ThoiGian = DateTime.Now,
                    DiaChiIP =  "0.0.0.0"
                };

                _context.NhatKyHeThongs.Add(nhatKy);

                await _context.SaveChangesAsync();
                return (true, "Lập phiếu nhập thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi lập phiếu: {ex.Message}");
            }
        }

        // ==========================================
        // 2. HÀM DUYỆT PHIẾU XUẤT (VÀ TRỪ TỒN KHO)
        // ==========================================
        // Hàm nhận vào ID của tờ phiếu xuất cần duyệt
        public async Task<(bool IsSuccess, string Message)> DuyetPhieuXuatAsync(int phieuXuatId, int taiKhoanId = 1)
        {
            try
            {
                var phieu = await _context.PhieuXuats
                    .Include(p => p.ChiTietPhieuXuats)
                    .FirstOrDefaultAsync(p => p.Id == phieuXuatId);
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