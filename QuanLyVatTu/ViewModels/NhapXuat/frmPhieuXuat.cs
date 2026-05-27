using System.Collections.Generic; // Khai báo dùng danh sách (List)
using System.ComponentModel.DataAnnotations; // Khai báo dùng các điều kiện bắt lỗi Form
using QuanLyVatTu.Models; // Khai báo lấy các class CSDL

namespace QuanLyVatTu.ViewModels.NhapXuat
{
    // Hộp chứa dữ liệu cho màn hình Lập Phiếu Xuất Kho
    public class frmPhieuXuat
    {
        // =========================================================
        // PHẦN 1: THÔNG TIN CHUNG
        // =========================================================

        [Display(Name = "Người nhận / Đơn vị nhận")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận vật tư")]
        public string NguoiNhan { get; set; } // Hứng tên người nhận hàng

        [Display(Name = "Lý do xuất kho")]
        [Required(ErrorMessage = "Vui lòng nhập lý do (VD: Xuất thi công công trình A)")]
        public string LyDoXuat { get; set; } // Hứng lý do xuất hàng

        // =========================================================
        // PHẦN 2: DANH SÁCH CHI TIẾT (CÁC MẶT HÀNG XUẤT)
        // =========================================================

        // Danh sách các dòng vật tư mà người dùng muốn xuất
        public List<ChiTietXuatVM> DanhSachChiTiet { get; set; } = new List<ChiTietXuatVM>();

        // =========================================================
        // PHẦN 3: DỮ LIỆU MỒI (ĐỂ VẼ GIAO DIỆN DROPDOWN LIST)
        // =========================================================

        // Khi xuất kho chỉ cần danh sách vật tư để chọn, không cần nhà cung cấp
        public List<VatTu> DanhSachVatTu { get; set; }
    }

    // =========================================================
    // CLASS PHỤ: ĐẠI DIỆN CHO 1 DÒNG VẬT TƯ MUỐN XUẤT
    // =========================================================
    public class ChiTietXuatVM
    {
        [Required(ErrorMessage = "Vui lòng chọn vật tư")]
        public int Id_VatTu { get; set; } // ID vật tư cần xuất

        [Required(ErrorMessage = "Vui lòng nhập số lượng xuất")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng xuất phải lớn hơn 0")]
        public int SoLuongXuat { get; set; } // Số lượng muốn rút khỏi kho

        // LƯU Ý: Ở form Xuất, ta không tạo biến DonGia. 
        // Đơn giá xuất sẽ được Server tự lấy từ trường GiaVonTB của bảng Vật Tư trong Database để tính toán.
        // Điều này ngăn chặn nhân viên tự ý sửa giá xuất hàng.
    }
}