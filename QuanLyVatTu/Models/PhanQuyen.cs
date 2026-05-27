namespace QuanLyVatTu.Models
{
    public class PhanQuyen
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<NguoiDung>? Users { get; set; }
    }
}
