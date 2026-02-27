using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;

namespace HotelManagementSystem.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public LoginModel(HotelManagementDbContext context) => _context = context;

        [BindProperty]
        public LoginInput LoginData { get; set; } = new();

        public class LoginInput
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["Message"] = message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == LoginData.Username && u.PasswordHash == LoginData.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
                return Page();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign In
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

            // Redirect based on Role
            if (user.Role == "Staff")
            {
                return RedirectToPage("/Staffs/MyTasks");
            }

            if (user.Role == "Technician")
            {
                return RedirectToPage("/Staffs/MaintenanceTasks");
            }

            // Both Consumer and Admin go to Index (Admin can view all, Consumer views limited)
            return RedirectToPage("/Index");
        }
    }
}
