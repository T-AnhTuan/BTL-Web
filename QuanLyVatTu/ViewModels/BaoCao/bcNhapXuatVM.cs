using System;
using System.Collections.Generic;
using QuanLyVatTu.Models; // Import namespace chứa các class CSDL (như PhieuNhap, PhieuXuat)

namespace QuanLyVatTu.ViewModels.BaoCao
{
    // Class này gom 6 món dữ liệu khác nhau vào chung một hộp để gửi ra View bcNhapXuat.cshtml
    public class bcNhapXuatVM
    {
        // 1. Chứa danh sách các phiếu nhập kho thỏa mãn điều kiện lọc theo thời gian
        public List<PhieuNhap> DanhSachNhap { get; set; } = new List<PhieuNhap>();

        // 2. Chứa danh sách các phiếu xuất kho (đã được duyệt) trong khoảng thời gian lọc
        public List<PhieuXuat> DanhSachXuat { get; set; } = new List<PhieuXuat>();

        // 3. Tính sẵn tổng số tiền đã chi ra để nhập hàng (Tổng giá trị các phiếu nhập)
        public decimal TongTienNhap { get; set; }

        // 4. Tính sẵn tổng số tiền thu được/giá trị xuất ra (Tổng giá trị các phiếu xuất)
        public decimal TongTienXuat { get; set; }

        // 5. Lưu lại giá trị bộ lọc: Ngày bắt đầu lấy báo cáo
        // (Để khi load lại View, ô Input [Từ ngày] vẫn giữ nguyên ngày người dùng đã chọn)
        public DateTime TuNgay { get; set; }

        // 6. Lưu lại giá trị bộ lọc: Ngày kết thúc lấy báo cáo
        // (Để khi load lại View, ô Input [Đến ngày] vẫn giữ nguyên ngày người dùng đã chọn)
        public DateTime DenNgay { get; set; }
    }
}