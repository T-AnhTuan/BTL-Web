using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu.Models
{
    public class PhieuNhap
    {
        [Key]//khóa chính
        public int Id { get; set; }

        //Thông tin chứng từ
        [Required]
        public DateTime NgayNhap { get; set; } = DateTime.Now;//ngày..
        public string SoPhieu { get; set; } = string.Empty;//số:...
        public string DonVi { get; set; }//đơn vị
        public string DiaChi { get; set; }//bộ phận

        //Đinh khoản kế toán
        public string TaiKhoanNo { get; set; }       // Nợ: ......
        public string TaiKhoanCo { get; set; }       // Có: ......

        //Thông tin giao nhận
        public string NhaCungCap { get; set; }   // Tên nhà cung cấp
        public string NhapTaiKho { get; set; }    // Tên kho: Nhập hàng
        public string DiaDiemKho { get; set; }       // Địa điểm kho

        //Thông tin kỹ nhận, tổng kết
        public int NguoiLapId { get; set; }
        [ForeignKey("NguoiLapId")]
        public NguoiDung NguoiLap { get; set; }    
        public string NguoiGiaoHang { get; set; }    
        public string ThuKho { get; set; }           
        public string KeToanTruong { get; set; }

        // Mối quan hệ 1 - Nhiều với Chi tiết phiếu nhập
        public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
           = new List<ChiTietPhieuNhap>();
    }
}
