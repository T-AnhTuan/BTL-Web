using System.ComponentModel.DataAnnotations;  // key, requird,StringLength 
using System.ComponentModel.DataAnnotations.Schema; //
namespace QuanLyVatTu.Models
{
    public class VaiTro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên Vai Trò")]
        public string TenVaiTro { get; set; } // VD: Quản trị viên, Thủ kho, Kế toán

        [StringLength(500)]
        [Display(Name = "Mô Tả")]
        public string MoTa { get; set; }

        // Navigation Properties
        public ICollection<TaiKhoan> TaiKhoans { get; set; }
        public ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}