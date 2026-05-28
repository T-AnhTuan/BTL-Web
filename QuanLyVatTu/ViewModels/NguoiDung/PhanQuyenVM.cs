using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels.NguoiDung
{
    /// <summary>
    /// ViewModel cho trang Quản lý Vai trò và Phân quyền (PhanQuyen.cshtml)
    /// </summary>
    public class PhanQuyenVM
    {
        [Display(Name = "ID Vai Trò")]
        public int Id { get; set; }

        [Display(Name = "Tên Nhóm Quyền")]
        [Required(ErrorMessage = "Tên nhóm quyền không được để trống")]
        [StringLength(30)]
        public string TenNhomQuyen { get; set; }

        [Display(Name = "Mô Tả")]
        [StringLength(500)]
        public string MoTa { get; set; }

        [Display(Name = "Số lượng người dùng")]
        public int SoLuongNguoiDung { get; set; }

        // Danh sách quyền chi tiết (nếu cần)
        public List<string> DanhSachQuyen { get; set; } = new();

        // Danh sách người dùng thuộc nhóm này
        public List<NguoiDungQuanLyVM> DanhSachNguoiDung { get; set; } = new();
    }

    /// <summary>
    /// ViewModel con: Thông tin người dùng trong danh sách quản lý
    /// </summary>
    public class NguoiDungQuanLyVM
    {
        public int Id { get; set; }

        [Display(Name = "Tên Tài Khoản")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Họ Tên")]
        public string HoTen { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Trạng Thái")]
        public bool IsKhóa { get; set; }

        [Display(Name = "Ngày Tạo")]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Vai Trò")]
        public string VaiTro { get; set; }
    }
}
