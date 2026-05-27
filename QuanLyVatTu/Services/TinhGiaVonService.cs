using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;

namespace QuanLyVatTu.Services
{
    // -------------------------------------------------------------------------
    // BƯỚC 1: TẠO INTERFACE (Để sử dụng Dependency Injection trong Program.cs)
    // -------------------------------------------------------------------------
    public interface ITinhGiaVonService
    {
        // Hàm tính lại giá vốn cho tất cả vật tư có trong một Phiếu Nhập cụ thể
        Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId);
    }

    // -------------------------------------------------------------------------
    // BƯỚC 2: TRIỂN KHAI LOGIC TÍNH TOÁN VÀO CLASS SERVICE
    // -------------------------------------------------------------------------
    public class TinhGiaVonService : ITinhGiaVonService
    {
        // Biến cục bộ để kết nối và tương tác với SQL Server (Database Context)
        private readonly AppDbContext _context;

        // Constructor Injection: Yêu cầu ASP.NET Core cung cấp DbContext
        public TinhGiaVonService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thuật toán: Bình quân gia quyền liên hoàn
        /// Công thức: Giá_Vốn_Mới = (Tổng_giá_trị_tồn_cũ + Tổng_giá_trị_nhập_mới) / (Số_lượng_tồn_cũ + Số_lượng_nhập_mới)
        /// Hàm này nên được gọi trong PhieuNhapController -> ở hàm [HttpPost] PheDuyetPhieuNhap()
        /// </summary>
        /// <param name="phieuNhapId">ID của Phiếu Nhập vừa được lưu/phê duyệt</param>
        public async Task<bool> TinhGiaVonBinhQuanSauKhiNhapAsync(int phieuNhapId)
        {
            try
            {
                // Lấy ra danh sách các chi tiết vật tư nằm trong Phiếu Nhập này
                // Model: ChiTietPhieuNhap (chứa Id_VatTu, SoLuongNhap, DonGiaNhap)
                var chiTietNhaps = await _context.ChiTietPhieuNhaps
                    .Where(ct => ct.PhieuNhapId == phieuNhapId)
                    .ToListAsync();

                if (!chiTietNhaps.Any())
                {
                    return false; // Phiếu nhập rỗng, không có gì để tính
                }

                // Duyệt qua từng dòng chi tiết (từng mã vật tư) trong phiếu nhập
                foreach (var chiTiet in chiTietNhaps)
                {
                    // Lấy thông tin Vật Tư hiện tại từ Database
                    // Model: VatTu (chứa Id, TenVatTu, SoLuongTon, GiaVonTB)
                    var vatTu = await _context.VatTus.FindAsync(chiTiet.Id);

                    if (vatTu != null)
                    {
                        // ----- BẮT ĐẦU TÍNH TOÁN BÌNH QUÂN GIA QUYỀN -----

                        // 1. Tính TỔNG GIÁ TRỊ TỒN KHO CŨ trước khi nhập
                        // (Lấy Số lượng tồn hiện tại * Giá vốn trung bình hiện tại)
                        decimal tongGiaTriTonCu = vatTu.SoLuongTon * vatTu.GiaVonTB;

                        // 2. Tính TỔNG GIÁ TRỊ LÔ HÀNG VỪA NHẬP MỚI
                        // (Lấy Số lượng thực nhập * Đơn giá nhập của nhà cung cấp trên bill)
                        decimal tongGiaTriNhapMoi = chiTiet.SoLuongThucNhap * chiTiet.DonGia;

                        // 3. Tính TỔNG SỐ LƯỢNG MỚI (Sau khi cộng dồn hàng mới vào kho)
                        int tongSoLuongMoi = vatTu.SoLuongTon + chiTiet.SoLuongThucNhap;

                        // 4. CHIA BÌNH QUÂN ĐỂ RA GIÁ VỐN MỚI
                        decimal giaVonMoi = 0;
                        if (tongSoLuongMoi > 0)
                        {
                            giaVonMoi = (tongGiaTriTonCu + tongGiaTriNhapMoi) / tongSoLuongMoi;
                        }

                        // ----- CẬP NHẬT LẠI VÀO DATABASE -----

                        // Cập nhật giá vốn mới cho vật tư
                        // (Giá này sẽ được dùng làm Đơn Giá Xuất khi lập Phiếu Xuất Kho)
                        vatTu.GiaVonTB = Math.Round(giaVonMoi, 2); // Làm tròn 2 số thập phân

                        // Cập nhật tăng số lượng tồn kho
                        vatTu.SoLuongTon = tongSoLuongMoi;

                        // Đánh dấu dòng dữ liệu Vật Tư này đã bị thay đổi
                        _context.VatTus.Update(vatTu);
                    }
                }

                // Lưu toàn bộ các thay đổi (Cập nhật số lượng & Giá vốn của nhiều vật tư) xuống SQL Server
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Nơi đây có thể kết hợp ghi Log (Bảng NhatKyHeThong) nếu lỗi
                Console.WriteLine($"Lỗi khi tính giá vốn: {ex.Message}");
                return false;
            }
        }
    }
}