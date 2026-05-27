using System.Collections.Generic;
using QuanLyVatTu.Models; // Import namespace chứa các class CSDL (như VatTu)

namespace QuanLyVatTu.ViewModels.BaoCao
{
    // Class này đóng vai trò là "gói dữ liệu" để gửi từ BaoCaoController ra View bcTonKho.cshtml
    public class bcTonKhoVM
    {
        // 1. Chứa danh sách toàn bộ vật tư trong kho để View dùng vòng lặp (foreach) in ra bảng
        // Khởi tạo sẵn một List rỗng để tránh lỗi NullReferenceException nếu kho chưa có hàng
        public List<VatTu> DanhSachVatTu { get; set; } = new List<VatTu>();

        // 2. Chứa con số: Tổng số loại mặt hàng (vật tư) đang được quản lý trong hệ thống
        // (Dùng để hiển thị lên thẻ Thống kê trên cùng của View)
        public int TongSoLoaiVatTu { get; set; }

        // 3. Chứa con số: Tổng giá trị tiền của toàn bộ kho hiện tại
        // (Tính bằng công thức: Số lượng tồn * Giá vốn bình quân của từng vật tư cộng lại)
        public decimal TongGiaTriTonKho { get; set; }

        // 4. Chứa con số: Số lượng các mặt hàng đang rơi vào tình trạng nguy hiểm (Sắp hết hàng)
        // (Ví dụ: Số lượng tồn <= 10)
        public int CanhBaoHetHang { get; set; }
    }
}