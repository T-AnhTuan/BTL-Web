using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyVatTu.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new RedirectToActionResult("Login", "Account", 
                    new { returnUrl = context.HttpContext.Request.Path.Value });
                return;
            }

            var hasAllowedRole = _allowedRoles.Any(role => user.IsInRole(role));

            if (!hasAllowedRole)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            await Task.CompletedTask;
        }
    }
}
