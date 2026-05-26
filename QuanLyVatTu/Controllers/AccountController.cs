using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using QuanLyVatTu.Services;
using QuanLyVatTu.ViewModels;
using System.Security.Claims;
using IAuthenticationService = QuanLyVatTu.Services.IAuthenticationService;

namespace QuanLyVatTu.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthenticationService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác");
                _logger.LogWarning($"Đăng nhập không thành công cho tài khoản: {model.Username}");
                return View(model);
            }

            // Create claims for user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FullName),
                new(ClaimTypes.Role, user.Role?.RoleName ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Login");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Signin
            await HttpContext.SignInAsync("CookieAuthentication", claimsPrincipal, 
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            await _authService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation($"Người dùng {user.Username} đã đăng nhập thành công");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuthentication");
            _logger.LogInformation($"Người dùng {User.Identity?.Name} đã đăng xuất");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
