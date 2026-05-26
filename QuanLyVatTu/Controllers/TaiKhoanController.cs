using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Data;
using QuanLyVatTu.Models;

namespace QuanLyVatTu.Controllers
{
    [Authorize]
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // Chỉ Admin có thể xem danh sách tài khoản
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _context.Users.Include(u => u.Role).ToList();
            return View(users);
        }

        // Xem thông tin tài khoản cá nhân
        public IActionResult MyProfile()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // Chỉ Admin có thể tạo tài khoản
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // Chỉ Admin có thể tạo tài khoản
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "Vui lòng nhập mật khẩu");
                ViewBag.Roles = _context.Roles.ToList();
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedDate = DateTime.Now;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Chỉ Admin có thể chỉnh sửa tài khoản
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            ViewBag.Roles = _context.Roles.ToList();
            return View(user);
        }

        // Chỉ Admin có thể chỉnh sửa tài khoản
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string? newPassword)
        {
            if (id != user.Id)
                return NotFound();

            var existingUser = _context.Users.Find(id);
            if (existingUser == null)
                return NotFound();

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.RoleId = user.RoleId;
            existingUser.IsActive = user.IsActive;

            if (!string.IsNullOrEmpty(newPassword))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Chỉ Admin có thể xóa tài khoản
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
