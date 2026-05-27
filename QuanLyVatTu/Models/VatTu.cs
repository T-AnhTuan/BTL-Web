using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class VatTu
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string TenVatTu { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string DonViTinh { get; set; } = string.Empty;

        public int SoLuongTon { get; set; } = 0;

        public int TonToiThieu { get; set; }

        public int TonToiDa { get; set; }

        [StringLength(200)]
        public string? NhaCungCap { get; set; }
        // --- LIÊN KẾT N-1 ---
        // Một vật tư có thể xuất hiện trong nhiều dòng chi tiết phiếu nhập
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
            = new HashSet<ChiTietPhieuNhap>();
    }
}
