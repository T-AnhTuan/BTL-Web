using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Services
{
    // ==========================================
    // 1. INTERFACE
    // ==========================================
    public interface IThongBaoService
    {

        // Nhóm hàm phục vụ Dashboard / Header
        Task<List<ThongBao>> LayThongBaoChuaXemAsync(int taiKhoanId);
        Task<List<NhatKyHeThong>> LayNhatKyGiaoDichMoiNhatAsync(int soLuong = 10);
    }

    // ==========================================
    // 2. CLASS IMPLEMENTATION
    // ==========================================
    public class ThongBaoService : IThongBaoService
    {
        private readonly AppDbContext _context;

        public ThongBaoService(AppDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // HÀM 1: LẤY THÔNG BÁO CHO HEADER CỦA USER
        // --------------------------------------------------
        public async Task<List<ThongBao>> LayThongBaoChuaXemAsync(int taiKhoanId)
        {
            return await _context.ThongBaos
                .Where(t => t.TaiKhoanId == taiKhoanId && t.DaXem == false)
                .OrderByDescending(t => t.NgayTao)
                .Take(5) // Chỉ lấy 5 thông báo mới nhất
                .ToListAsync();
        }

        // --------------------------------------------------
        // HÀM 2: LẤY NHẬT KÝ CHO DASHBOARD
        // --------------------------------------------------
        public async Task<List<NhatKyHeThong>> LayNhatKyGiaoDichMoiNhatAsync(int soLuong = 10)
        {
            return await _context.NhatKyHeThongs
                .Include(n => n.TaiKhoan)
                    .ThenInclude(t => t.NhanVien) // Lấy ra tên nhân viên
                .OrderByDescending(n => n.ThoiGian)
                .Take(soLuong)
                .ToListAsync();
        }
    }
}