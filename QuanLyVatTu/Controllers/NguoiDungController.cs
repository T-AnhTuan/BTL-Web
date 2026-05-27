using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.ViewModels.DangNhap;
//using QuanLyVatTu.ViewModels.NguoiDung;
using System.Security.Claims;

namespace QuanLyVatTu.Controllers
{
    public class NguoiDungController : Controller
    {
        [HttpGet] // Giao diện (GET)
        public IActionResult DangNhap()
        {
            return View();
        }
        // Xử lý khi bấm nút Đăng nhập (POST)
        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapVM model)
        {
            
        }
        // Xử lý Đăng xuất
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap", "NguoiDung");
        }
    }
}
