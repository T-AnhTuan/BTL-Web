using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu.Services
{
    // =========================================================================
    // 1. TẠO INTERFACE (Để sử dụng Dependency Injection)
    // =========================================================================
    public interface INhapXuatService
    {
        Task<List<PhieuNhap>> LayDanhSachPhieuNhapAsync();
        // Trả về Tuple (bool, string) để biết thành công hay thất bại và kèm câu thông báo
        Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, string nguoiLap);

        Task<List<PhieuXuat>> LayDanhSachPhieuXuatAsync();
        Task<bool> LapPhieuXuatAsync(PhieuXuat phieuXuat);
        Task<(bool IsSuccess, string Message)> PheDuyetPhieuXuatAsync(int phieuXuatId, string nguoiDuyet);

        Task<List<VatTu>> LayDanhSachKiemKeAsync();
    }

    // =========================================================================
    // 2. TRIỂN KHAI LOGIC NGHIỆP VỤ
    // =========================================================================
    public class NhapXuatService : INhapXuatService
    {
        private readonly AppDbContext _context;
        private readonly ITinhGiaVonService _tinhGiaVonService;

        // Tiêm DbContext và TinhGiaVonService vào đây thay vì tiêm vào Controller
        public NhapXuatService(AppDbContext context, ITinhGiaVonService tinhGiaVonService)
        {
            _context = context;
            _tinhGiaVonService = tinhGiaVonService;
        }

        // --- NGHIỆP VỤ NHẬP KHO ---

        public async Task<List<PhieuNhap>> LayDanhSachPhieuNhapAsync()
        {
            // Lấy danh sách phiếu nhập kèm thông tin Nhà cung cấp
            return await _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
                .OrderByDescending(p => p.NgayNhap)
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, string Message)> LapPhieuNhapAsync(PhieuNhap phieuNhap, string nguoiLap)
        {
            try
            {
                // 1. Lưu thông tin Phiếu Nhập
                phieuNhap.NgayNhap = DateTime.Now;
                phieuNhap.NguoiLap = nguoiLap;
                phieuNhap.TrangThai = "Đã nhập kho";

                _context.PhieuNhaps.Add(phieuNhap);
                await _context.SaveChangesAsync(); // Lưu để phát sinh ID Phiếu Nhập

                // 2. Kích hoạt tính giá vốn tự động
                bool isTinhGiaVonSuccess = await _tinhGiaVonService.TinhGiaVonBinhQuanSauKhiNhapAsync(phieuNhap.Id);

                if (isTinhGiaVonSuccess)
                    return (true, "Lập phiếu nhập thành công. Đã cập nhật tồn kho và tính lại giá vốn!");
                else
                    return (true, "Lập phiếu nhập thành công nhưng phiếu trống (không có vật tư để tính giá vốn).");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // --- NGHIỆP VỤ XUẤT KHO ---

        public async Task<List<PhieuXuat>> LayDanhSachPhieuXuatAsync()
        {
            return await _context.PhieuXuats
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();
        }

        public async Task<bool> LapPhieuXuatAsync(PhieuXuat phieuXuat)
        {
            phieuXuat.NgayXuat = DateTime.Now;
            phieuXuat.TrangThai = "Chờ phê duyệt";

            _context.PhieuXuats.Add(phieuXuat);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool IsSuccess, string Message)> PheDuyetPhieuXuatAsync(int phieuXuatId, string nguoiDuyet)
        {
            var phieu = await _context.PhieuXuats
                                      .Include(p => p.ChiTietPhieuXuats)
                                      .FirstOrDefaultAsync(p => p.Id == phieuXuatId);

            if (phieu == null || phieu.TrangThai == "Đã duyệt")
            {
                return (false, "Phiếu không tồn tại hoặc đã được duyệt rồi.");
            }

            // Tiến hành trừ Tồn Kho thực tế
            foreach (var chiTiet in phieu.ChiTietPhieuXuats)
            {
                var vatTu = await _context.VatTus.FindAsync(chiTiet.VatTuId);
                if (vatTu != null)
                {
                    // Kiểm tra tồn kho trước khi duyệt
                    if (vatTu.SoLuongTon < chiTiet.SoLuongYeuCau)
                    {
                        return (false, $"Lỗi: Vật tư {vatTu.TenVatTu} không đủ tồn kho để xuất!");
                    }

                    // Trừ tồn kho và cập nhật
                    vatTu.SoLuongTon -= chiTiet.SoLuongYeuCau;
                    _context.VatTus.Update(vatTu);
                }
            }

            // Cập nhật trạng thái phiếu xuất
            phieu.TrangThai = "Đã duyệt";
            phieu.NguoiDuyet = nguoiDuyet;
            _context.PhieuXuats.Update(phieu);

            await _context.SaveChangesAsync();

            return (true, "Đã phê duyệt phiếu xuất và trừ tồn kho thành công!");
        }

        // --- NGHIỆP VỤ KIỂM KÊ ---

        public async Task<List<VatTu>> LayDanhSachKiemKeAsync()
        {
            return await _context.VatTus.ToListAsync();
        }
    }
}