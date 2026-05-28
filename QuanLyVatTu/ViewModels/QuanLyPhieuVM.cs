using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Quản lý Phiếu Nhập (PhieuNhap.cshtml)
    /// Hiển thị danh sách phiếu nhập + thống kê
    /// </summary>
    public class QuanLyPhieuNhapVM
    {
        // --- Danh sách phiếu nhập ---
        public List<PhieuNhapItemVM> DanhSachPhieuNhap { get; set; } = new();

        // --- Bộ lọc ---
        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Từ Ngày")]
        public DateTime? TuNgay { get; set; }

        [Display(Name = "Đến Ngày")]
        public DateTime? DenNgay { get; set; }

        [Display(Name = "Nhà Cung Cấp")]
        public string NhaCungCap { get; set; }

        [Display(Name = "Trạng Thái")]
        public string TrangThai { get; set; }

        // --- Thống kê ---
        public int TongPhieuNhap { get; set; }
        public decimal TongGiaTriNhap { get; set; }
        public int PhieuChoPheDuyet { get; set; }
        public int PhieuDaPheDuyet { get; set; }
    }

    /// <summary>
    /// Item trong danh sách Phiếu Nhập
    /// </summary>
    public class PhieuNhapItemVM
    {
        public int Id { get; set; }

        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Ngày Nhập")]
        public DateTime NgayNhap { get; set; }

        [Display(Name = "Nhà Cung Cấp")]
        public string NhaCungCap { get; set; }

        [Display(Name = "Người Lập")]
        public string NguoiLap { get; set; }

        [Display(Name = "Số Dòng")]
        public int SoDong { get; set; }

        [Display(Name = "Tổng Giá Trị")]
        public decimal TongGiaTriNhap { get; set; }

        [Display(Name = "Trạng Thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Ghi Chú")]
        public string GhiChu { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang Lập Phiếu Nhập (form + chi tiết)
    /// </summary>
    public class LapPhieuNhapVM
    {
        // --- Thông tin phiếu nhập ---
        public int Id { get; set; }

        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Ngày Nhập")]
        [Required(ErrorMessage = "Ngày nhập không được để trống")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        [Display(Name = "Nhà Cung Cấp")]
        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        [StringLength(200)]
        public string NhaCungCap { get; set; }

        [Display(Name = "Đơn Vị")]
        [StringLength(100)]
        public string DonVi { get; set; }

        [Display(Name = "Địa Chỉ")]
        [StringLength(200)]
        public string DiaChi { get; set; }

        [Display(Name = "Tài Khoản Nợ")]
        [StringLength(50)]
        public string TaiKhoanNo { get; set; }

        [Display(Name = "Tài Khoản Có")]
        [StringLength(50)]
        public string TaiKhoanCo { get; set; }

        [Display(Name = "Địa Điểm Kho")]
        [StringLength(100)]
        public string DiaDiemKho { get; set; }

        // --- Chi tiết phiếu nhập ---
        public List<ChiTietPhieuNhapItemVM> ChiTietNhap { get; set; } = new();

        // --- Thống kê ---
        public int TongSoDong => ChiTietNhap.Count;
        public decimal TongGiaTriNhap => ChiTietNhap.Sum(x => x.ThanhTien);

        // --- Ký nhân ---
        [Display(Name = "Người Giao Hàng")]
        [StringLength(100)]
        public string NguoiGiaoHang { get; set; }

        [Display(Name = "Thủ Kho")]
        [StringLength(100)]
        public string ThuKho { get; set; }

        [Display(Name = "Kế Toán Trưởng")]
        [StringLength(100)]
        public string KeToanTruong { get; set; }
    }

    /// <summary>
    /// Chi tiết một dòng trong Phiếu Nhập
    /// </summary>
    public class ChiTietPhieuNhapItemVM
    {
        public int Id { get; set; }

        [Display(Name = "STT")]
        public int STT { get; set; }

        [Display(Name = "Vật Tư")]
        [Required(ErrorMessage = "Vật tư không được để trống")]
        public int VatTuId { get; set; }

        public string TenVatTu { get; set; }
        public string DonViTinh { get; set; }

        [Display(Name = "Mã Số")]
        [StringLength(50)]
        public string MaSo { get; set; }

        [Display(Name = "SL Theo CT")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public decimal SoLuongTheoChungTu { get; set; }

        [Display(Name = "SL Thực Nhập")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public decimal SoLuongThucNhap { get; set; }

        [Display(Name = "Đơn Giá")]
        [Range(0, double.MaxValue)]
        public decimal DonGia { get; set; }

        [Display(Name = "Thành Tiền")]
        public decimal ThanhTien => SoLuongThucNhap * DonGia;

        [Display(Name = "Ghi Chú")]
        [StringLength(500)]
        public string GhiChu { get; set; }
    }

    /// <summary>
    /// ViewModel tương tự cho Phiếu Xuất
    /// </summary>
    public class QuanLyPhieuXuatVM
    {
        public List<PhieuXuatItemVM> DanhSachPhieuXuat { get; set; } = new();

        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Từ Ngày")]
        public DateTime? TuNgay { get; set; }

        [Display(Name = "Đến Ngày")]
        public DateTime? DenNgay { get; set; }

        [Display(Name = "Người Mua")]
        public string NguoiMua { get; set; }

        public int TongPhieuXuat { get; set; }
        public decimal TongGiaTriXuat { get; set; }
    }

    public class PhieuXuatItemVM
    {
        public int Id { get; set; }

        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Ngày Xuất")]
        public DateTime NgayXuat { get; set; }

        [Display(Name = "Người Mua")]
        public string NguoiMua { get; set; }

        [Display(Name = "Lý Do Xuất")]
        public string LyDoXuat { get; set; }

        [Display(Name = "Tổng Giá Trị")]
        public decimal TongGiaTriXuat { get; set; }
    }

    public class LapPhieuXuatVM
    {
        public int Id { get; set; }

        [Display(Name = "Số Phiếu")]
        public string SoPhieu { get; set; }

        [Display(Name = "Ngày Xuất")]
        [Required]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [Display(Name = "Người Mua")]
        [Required]
        [StringLength(100)]
        public string NguoiMua { get; set; }

        [Display(Name = "Lý Do Xuất")]
        [StringLength(200)]
        public string LyDoXuat { get; set; }

        [Display(Name = "Địa Điểm Kho")]
        [StringLength(100)]
        public string DiaDiemKho { get; set; }

        public List<ChiTietPhieuXuatItemVM> ChiTietXuat { get; set; } = new();

        public decimal TongGiaTriXuat => ChiTietXuat.Sum(x => x.ThanhTien);
    }

    public class ChiTietPhieuXuatItemVM
    {
        public int Id { get; set; }

        [Display(Name = "STT")]
        public int STT { get; set; }

        [Display(Name = "Vật Tư")]
        public int VatTuId { get; set; }

        public string TenVatTu { get; set; }
        public string DonViTinh { get; set; }

        [Display(Name = "SL Yêu Cầu")]
        public decimal SoLuongYeuCau { get; set; }

        [Display(Name = "SL Thực Xuất")]
        public decimal SoLuongThucXuat { get; set; }

        [Display(Name = "Đơn Giá")]
        public decimal DonGia { get; set; }

        [Display(Name = "Thành Tiền")]
        public decimal ThanhTien => SoLuongThucXuat * DonGia;
    }
}
