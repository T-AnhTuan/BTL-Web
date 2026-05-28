using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu.Models
{
    public class PhanQuyen
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết với Vai Trò
        [Required]
        public int VaiTroId { get; set; }
        [ForeignKey("VaiTroId")]
        public VaiTro VaiTro { get; set; }

        // Mã chức năng/Module (VD: "QL_NHAPKHO", "QL_XUATKHO", "QL_DANHMUC", "BAOCAO")
        [Required]
        [StringLength(50)]
        [Display(Name = "Mã Chức Năng")]
        public string MaChucNang { get; set; }

        [Display(Name = "Tên Chức Năng")]
        [StringLength(100)]
        public string TenChucNang { get; set; } // Phục vụ hiển thị trên giao diện (VD: "Quản lý Phiếu Nhập")

        // 4 Cờ (Flags) phân quyền chi tiết (Checkboxes trên giao diện)
        [Display(Name = "Quyền Xem")]
        public bool CoQuyenXem { get; set; } = false;

        [Display(Name = "Quyền Thêm Mới")]
        public bool CoQuyenThem { get; set; } = false;

        [Display(Name = "Quyền Chỉnh Sửa")]
        public bool CoQuyenSua { get; set; } = false;

        [Display(Name = "Quyền Xóa")]
        public bool CoQuyenXoa { get; set; } = false;
    }
}
