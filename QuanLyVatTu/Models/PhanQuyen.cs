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

       // Mối quan hệ: Một quyền có nhiều Người dùng
       public ICollection<NguoiDung> NguoiDung { get; set; }
         = new List<NguoiDung>();
    }
}
