using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu.ViewModels
{
    // ==========================================
    // 1. VIEW MODEL CHÍNH CHO TOÀN TRANG
    // Dùng để hứng dữ liệu bộ lọc từ Form và truyền List Data ra Bảng
    // ==========================================
    public class BaoCaoVM
    {
        // --- PHẦN 1: BỘ LỌC TÌM KIẾM (FILTER) ---
        [Display(Name = "Kho")]
        public int? KhoId { get; set; }

        [Display(Name = "Từ Ngày")]
        [DataType(DataType.Date)]
        public DateTime? TuNgay { get; set; }

        [Display(Name = "Đến Ngày")]
        [DataType(DataType.Date)]
        public DateTime? DenNgay { get; set; }

        [Display(Name = "Từ khóa (Mã/Tên VT)")]
        public string TuKhoa { get; set; }

        // Danh sách kho để hiển thị ra thẻ <select> trên giao diện
        // public SelectList DanhSachKho { get; set; } 


        // --- PHẦN 2: DỮ LIỆU BẢNG (RUỘT BẢNG) ---
        public List<BaoCaoItem> DanhSachChiTiet { get; set; } = new List<BaoCaoItem>();


        // --- PHẦN 3: DỮ LIỆU TỔNG CỘNG (FOOTER BẢNG) ---
        // Sử dụng LINQ để tự động cộng tổng từ DanhSachChiTiet, View HTML chỉ việc gọi ra hiển thị
        public int TongTonDauSoLuong => DanhSachChiTiet.Sum(x => x.TonDauSoLuong);
        public decimal TongTonDauGiaTri => DanhSachChiTiet.Sum(x => x.TonDauGiaTri);

        public int TongNhapSoLuong => DanhSachChiTiet.Sum(x => x.NhapSoLuong);
        public decimal TongNhapGiaTri => DanhSachChiTiet.Sum(x => x.NhapGiaTri);

        public int TongXuatSoLuong => DanhSachChiTiet.Sum(x => x.XuatSoLuong);
        public decimal TongXuatGiaTri => DanhSachChiTiet.Sum(x => x.XuatGiaTri);

        public int TongTonCuoiSoLuong => DanhSachChiTiet.Sum(x => x.TonCuoiSoLuong);
        public decimal TongTonCuoiGiaTri => DanhSachChiTiet.Sum(x => x.TonCuoiGiaTri);
    }

    // ==========================================
    // 2. DTO ĐẠI DIỆN CHO 1 DÒNG TRONG BẢNG
    // ==========================================
    public class BaoCaoItem
    {
        public int VatTuId { get; set; }

        [Display(Name = "Mã Vật Tư")]
        public string MaVatTu { get; set; }

        [Display(Name = "Tên Vật Tư")]
        public string TenVatTu { get; set; }

        [Display(Name = "ĐVT")]
        public string DonViTinh { get; set; }

        // --- TỒN ĐẦU KỲ ---
        public int TonDauSoLuong { get; set; } = 0;
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TonDauGiaTri { get; set; } = 0;

        // --- NHẬP TRONG KỲ ---
        public int NhapSoLuong { get; set; } = 0;
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal NhapGiaTri { get; set; } = 0;

        // --- XUẤT TRONG KỲ ---
        public int XuatSoLuong { get; set; } = 0;
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal XuatGiaTri { get; set; } = 0;

        // --- TỒN CUỐI KỲ ---
        // Thường được tính theo công thức: Tồn Đầu + Nhập - Xuất
        public int TonCuoiSoLuong => TonDauSoLuong + NhapSoLuong - XuatSoLuong;
        public decimal TonCuoiGiaTri => TonDauGiaTri + NhapGiaTri - XuatGiaTri;
    }
}