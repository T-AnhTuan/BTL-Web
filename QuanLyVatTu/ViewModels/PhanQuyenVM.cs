using QuanLyVatTu.Models;

namespace QuanLyVatTu.ViewModels
{
    public class PhanQuyenVM
    {
        // Danh sách hiển thị ở Sidebar bên trái
        public List<VaiTro> DanhSachVaiTro { get; set; }

        // Vai trò đang được chọn để cấu hình ở bảng bên phải
        public VaiTro VaiTroDangChon { get; set; }
    }
}