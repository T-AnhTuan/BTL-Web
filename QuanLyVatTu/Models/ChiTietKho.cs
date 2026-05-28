using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu.Models
{
    public class ChiTietKho
    {
        [Key]
        public int Id { get; set; }

        public int KhoId { get; set; }
        [ForeignKey("KhoId")]
        public DanhMucKho Kho { get; set; }

        public int VatTuId { get; set; }
        [ForeignKey("VatTuId")]
        public VatTu VatTu { get; set; }

        [Display(Name = "Số Lượng Tồn")]
        public int SoLuong { get; set; } = 0;
    }
}
