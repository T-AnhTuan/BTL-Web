namespace QuanLyVatTu.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<User>? Users { get; set; }
    }
}
