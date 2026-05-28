using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels.VatTu
{
    /// <summary>
    /// ViewModel cho trang Danh Sách Vật Tư (dsVatTu.cshtml)
    /// Dùng để hiển thị danh sách + filter + thêm/sửa/xóa
    /// </summary>
    public class DanhSachVatTuVM
    {
        // --- Dữ liệu danh sách ---
        public List<VatTuItemVM> DanhSachVatTu { get; set; } = new();

        // --- Filter/Search ---
        [Display(Name = "Tên Vật Tư")]
        public string TenVatTu { get; set; }

        [Display(Name = "Đơn Vị Tính")]
        public string DonViTinh { get; set; }

        [Display(Name = "Nhà Cung Cấp")]
        public string NhaCungCap { get; set; }

        [Display(Name = "Từ Số Lượng")]
        public int? SoLuongTu { get; set; }

        [Display(Name = "Đến Số Lượng")]
        public int? SoLuongDen { get; set; }

        // --- Tổng hợp thống kê ---
        public int TongSoVatTu { get; set; }
        public int TongSoLuongTon { get; set; }
        public decimal TongGiaTriTon { get; set; }
        public int SoVatTuDuoiMucToiThieu { get; set; }
    }

    /// <summary>
    /// Item trong danh sách Vật Tư
    /// </summary>
    public class VatTuItemVM
    {
        public int Id { get; set; }

        [Display(Name = "Tên Vật Tư")]
        public string TenVatTu { get; set; }

        [Display(Name = "Mã VT")]
        public string MaVatTu { get; set; }

        [Display(Name = "Đơn Vị")]
        public string DonViTinh { get; set; }

        [Display(Name = "Số Lượng Tồn")]
        public int SoLuongTon { get; set; }

        [Display(Name = "Tồn Tối Thiểu")]
        public int TonToiThieu { get; set; }

        [Display(Name = "Tồn Tối Đa")]
        public int TonToiDa { get; set; }

        [Display(Name = "Nhà Cung Cấp")]
        public string NhaCungCap { get; set; }

        [Display(Name = "Trạng Thái")]
        public string TrangThai { get; set; } // "Bình thường", "Cảnh báo", "Thiếu hàng"

        [Display(Name = "Giá Vốn TB")]
        public decimal GiaVonTrungBinh { get; set; }

        [Display(Name = "Giá Trị Tồn")]
        public decimal GiaTriTon => SoLuongTon * GiaVonTrungBinh;
    }

    /// <summary>
    /// ViewModel cho form Thêm/Sửa Vật Tư
    /// </summary>
    public class ThemSuaVatTuVM
    {
        public int Id { get; set; }

        [Display(Name = "Tên Vật Tư")]
        [Required(ErrorMessage = "Tên vật tư không được để trống")]
        [StringLength(100)]
        public string TenVatTu { get; set; }

        [Display(Name = "Đơn Vị Tính")]
        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(20)]
        public string DonViTinh { get; set; }

        [Display(Name = "Tồn Tối Thiểu")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải ≥ 0")]
        public int TonToiThieu { get; set; }

        [Display(Name = "Tồn Tối Đa")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải ≥ 0")]
        public int TonToiDa { get; set; }

        [Display(Name = "Nhà Cung Cấp")]
        [StringLength(200)]
        public string NhaCungCap { get; set; }

        [Display(Name = "Ghi Chú")]
        [StringLength(500)]
        public string GhiChu { get; set; }
    }
}
