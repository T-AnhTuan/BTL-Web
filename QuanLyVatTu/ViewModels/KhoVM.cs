namespace QuanLyVatTu.ViewModels
{
    // Lớp này dùng để chứa toàn bộ dữ liệu gửi ra trang Kho.cshtml
    public class KhoVM
    {
        public int? VatTuId { get; set; }
        public string TenVatTu { get; set; }
        public string TenKho { get; set; }

        // Danh sách chi tiết các dòng giao dịch nhập/xuất để in ra bảng
        public List<ChiTietGiaoDichViewModel> ChiTietGiaoDich { get; set; } = new List<ChiTietGiaoDichViewModel>();
    }

    // Lớp này đại diện cho 1 dòng (1 <tr>) trong bảng báo cáo
    public class ChiTietGiaoDichViewModel
    {
        public DateTime NgayGiaoDich { get; set; }
        public string SoChungTu { get; set; }
        public string DienGiai { get; set; }

        public string LoaiGiaoDich { get; set; } // "Nhập" hoặc "Xuất"

        public int SoLuong { get; set; }

        // Cột Tồn kho sau mỗi lần giao dịch (Được tính toán bằng C#)
        public int TonKhoSauGiaoDich { get; set; }

        public string GhiChu { get; set; }
    }
}