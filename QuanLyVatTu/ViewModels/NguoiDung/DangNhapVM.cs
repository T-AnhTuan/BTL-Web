using System.ComponentModel.DataAnnotations; // Thư viện cung cấp các thuộc tính kiểm tra dữ liệu (như [Required], [Display])

namespace QuanLyVatTu.ViewModels.DangNhap
{
    // Class này là "chiếc hộp" chứa đúng 3 thông tin cần thiết nhất để lấy từ Form Đăng nhập.
    // Việc dùng ViewModel giúp ta không phải mang toàn bộ Model NguoiDung (chứa cả họ tên, quyền hạn, trạng thái...) 
    // ra ngoài giao diện web, điều này giúp tăng cường tính bảo mật chống hacker truyền dữ liệu rác vào.
    public class DangNhapVM
    {
        // -------------------------------------------------------------
        // 1. TRƯỜNG TÊN ĐĂNG NHẬP
        // -------------------------------------------------------------

        // [Required]: Bắt buộc người dùng không được để trống. 
        // Nếu để trống ô input, form sẽ tự động chặn lại và báo dòng chữ màu đỏ ở ErrorMessage.
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập của bạn")]

        // [Display]: Khi ra ngoài HTML, nếu bạn dùng thẻ <label asp-for="TenDangNhap"></label>
        // Nó sẽ tự động biến thành chữ "Tên đăng nhập" mà bạn không cần gõ chữ cứng ngoài HTML.
        [Display(Name = "Tên đăng nhập")]

        // Khai báo biến TenDangNhap (Sửa lại từ chữ Username để khớp với Controller)
        public string TenDangNhap { get; set; } = string.Empty; // Gán mặc định là rỗng để tránh lỗi cảnh báo Null


        // -------------------------------------------------------------
        // 2. TRƯỜNG MẬT KHẨU
        // -------------------------------------------------------------

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]

        // [DataType(DataType.Password)]: Lệnh này cực kỳ quan trọng! 
        // Nó báo cho ASP.NET Core biết lúc sinh ra thẻ <input> ngoài HTML phải tự động gán thuộc tính type="password".
        // Từ đó người dùng gõ chữ vào sẽ tự biến thành các dấu chấm tròn hoặc dấu sao (*) che giấu mật khẩu.
        [DataType(DataType.Password)]

        // Khai báo biến MatKhau (Sửa lại từ chữ Password để khớp với Controller)
        public string MatKhau { get; set; } = string.Empty;


        // -------------------------------------------------------------
        // 3. TRƯỜNG GHI NHỚ ĐĂNG NHẬP
        // -------------------------------------------------------------

        [Display(Name = "Ghi nhớ đăng nhập")]
        // Biểu diễn dưới dạng boolean (true/false).
        // Khi ra ngoài giao diện, ASP.NET Core sẽ tự động vẽ một ô Checkbox (hộp kiểm) cho phép người dùng đánh dấu tick.
        public bool NhoMatKhau { get; set; }
    }
}