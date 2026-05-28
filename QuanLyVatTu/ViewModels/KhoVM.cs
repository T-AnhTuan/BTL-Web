using System;
using System.Collections.Generic;

namespace QuanLyVatTu.ViewModels
{
    // Đây là "Cái mâm" bưng toàn bộ dữ liệu ra trang Thẻ Kho
    public class KhoVM
    {
        public int VatTuId { get; set; }
        public string TenVatTu { get; set; }
        public string TenKho { get; set; }

        // Chứa danh sách các dòng giao dịch (vừa Nhập vừa Xuất)
        public List<ChiTietGiaoDichViewModel> ChiTietGiaoDich { get; set; }
            = new List<ChiTietGiaoDichViewModel>();
    }

    // Đây là từng dòng <tr> trong cái bảng Thẻ Kho
    public class ChiTietGiaoDichViewModel
    {
        public DateTime NgayGiaoDich { get; set; }
        public string SoChungTu { get; set; } // Ví dụ: PN001, PX005
        public string DienGiai { get; set; }

        public string LoaiGiaoDich { get; set; } // Chỉ nhận 2 giá trị: "Nhập" hoặc "Xuất"
        public int SoLuong { get; set; }

        // Số tồn này được tính toán động (Running Total) bằng C# lúc xuất báo cáo
        public int TonKhoSauGiaoDich { get; set; }
        public string GhiChu { get; set; }
    }
}