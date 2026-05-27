using System.Collections.Generic; // Gọi thư viện để dùng List<>
using System.ComponentModel.DataAnnotations; // Gọi thư viện để dùng các attribute kiểm tra lỗi (Required, Display...)
using QuanLyVatTu.Models; // Gọi namespace chứa các Models CSDL (VatTu, NhaCungCap)

namespace QuanLyVatTu.ViewModels.NhapXuat
{
    // Class này là cái khung để hứng toàn bộ dữ liệu từ Form "Lập Phiếu Nhập" gửi lên
    public class frmPhieuNhap
    {
        // =========================================================
        // PHẦN 1: THÔNG TIN CHUNG CỦA PHIẾU
        // =========================================================

        [Display(Name = "Nhà cung cấp")] // Nhãn hiển thị ngoài giao diện HTML
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")] // Bắt buộc người dùng không được để trống
        public int Id_NhaCungCap { get; set; } // Chứa ID của nhà cung cấp được chọn từ Dropdown

        [Display(Name = "Ghi chú / Số hóa đơn")]
        public string GhiChu { get; set; } // Người dùng gõ text ghi chú (có thể để trống nên không có [Required])

        // =========================================================
        // PHẦN 2: DANH SÁCH CHI TIẾT (CÁC MẶT HÀNG NHẬP)
        // =========================================================

        // Một phiếu nhập có thể nhập nhiều loại vật tư cùng lúc. 
        // Nên ta dùng một List<> chứa danh sách các dòng chi tiết.
        public List<ChiTietNhapVM> DanhSachChiTiet { get; set; } = new List<ChiTietNhapVM>();

        // =========================================================
        // PHẦN 3: DỮ LIỆU MỒI (ĐỂ VẼ GIAO DIỆN DROPDOWN LIST)
        // =========================================================

        // Chứa danh sách các nhà cung cấp lấy từ DB để View vẽ ra thẻ <select>
        public List<NhaCungCap> DanhSachNhaCungCap { get; set; }

        // Chứa danh sách các vật tư lấy từ DB để View vẽ ra thẻ <select> chọn mặt hàng
        public List<VatTu> DanhSachVatTu { get; set; }
    }

    // =========================================================
    // CLASS PHỤ: ĐẠI DIỆN CHO 1 DÒNG VẬT TƯ NHẬP VÀO
    // =========================================================
    public class ChiTietNhapVM
    {
        [Required(ErrorMessage = "Vui lòng chọn vật tư")]
        public int Id_VatTu { get; set; } // Hứng ID vật tư

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 1000000, ErrorMessage = "Số lượng nhập phải lớn hơn 0")] // Bắt lỗi nếu nhập số âm
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Range(1, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")] // Bắt lỗi nếu giá tiền âm
        public decimal DonGiaNhap { get; set; }
    }
}