using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyVatTu.Models
{
    public class VatTu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã vật tư là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Mã Vật Tư")]
        public string MaVatTu { get; set; }

        [Required(ErrorMessage = "Tên vật tư là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Tên Vật Tư")]
        public string TenVatTu { get; set; }

        // Khóa ngoại liên kết với Danh Mục
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh Mục")]
        public int DanhMucId { get; set; }
        [ForeignKey("DanhMucId")]
        public DanhMucVatTu DanhMuc { get; set; }

        [StringLength(50)]
        [Display(Name = "Đơn Vị Tính")]
        public string DonViTinh { get; set; }

        [Display(Name = "Tồn Kho Hiện Tại")]
        public int TonKhoHienTai { get; set; } = 0;

        [Display(Name = "Tồn Kho tối thiểu")]
        public int TonToiThieu { get; set; } = 0;
        [Display(Name = "Giá Vốn Bình Quân")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaVonBinhQuan { get; set; } = 0;
    }
}
