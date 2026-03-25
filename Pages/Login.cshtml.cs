using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MiddlewareEngine.Pages
{
    public class LoginModel : PageModel
    {
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If already authenticated, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string username, string password, bool rememberMe = false)
        {
            // Simple authentication - in production, use proper authentication service
            // Default credentials: admin/admin or operator/operator
            if ((username == "admin" && password == "admin") || 
                (username == "operator" && password == "operator"))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, username == "admin" ? "Administrator" : "Operator"),
                    new Claim("DisplayName", username == "admin" ? "Admin" : "Operator")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(rememberMe ? 24 : 8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid username or password. Try admin/admin or operator/operator.";
            return Page();
        }
    }
}
