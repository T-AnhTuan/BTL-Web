using System.ComponentModel.DataAnnotations;  // key, requird,StringLength 
using System.ComponentModel.DataAnnotations.Schema; //
namespace QuanLyVatTu.Models
{
    public class PhanQuyen
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string TenNhomQuyen { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        // M?i quan h?: M?t quy?n có nhi?u Ng??i dùng
        public ICollection<NguoiDung> NguoiDung { get; set; }
          = new List<NguoiDung>();
    }
}