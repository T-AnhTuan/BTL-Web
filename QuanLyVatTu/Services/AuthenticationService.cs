using QuanLyVatTu.Models;
using QuanLyVatTu.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu.Services
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> RegisterUserAsync(string username, string email, string password, string fullName, int roleId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task UpdateLastLoginAsync(int userId);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;

        public AuthenticationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password, string fullName, int roleId)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            if (existingUser != null)
                return false;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                RoleId = roleId,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
