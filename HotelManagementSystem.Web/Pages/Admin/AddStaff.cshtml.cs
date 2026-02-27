using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AddStaffModel : PageModel
    {
        private readonly AccountService _accountService;
        public AddStaffModel(AccountService accountService) => _accountService = accountService;

        [BindProperty]
        public StaffInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = new User
            {
                Username = Input.UserName,
                PasswordHash = Input.Password,
                Email = Input.Email,
                Role = Input.IsAdmin ? "Admin" : "Staff",
                FullName = Input.FullName,
                CreatedAt = DateTime.Now
            };

            var ok = await _accountService.RegisterStaff(user, Input.Position, Input.Shift);
            if (!ok)
            {
                ModelState.AddModelError("", "Không thể tạo tài khoản nhân viên.");
                return Page();
            }

            return RedirectToPage("/Index");
        }

        public class StaffInput
        {
            public string FullName { get; set; } = null!;
            public string UserName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string Position { get; set; } = "Lễ tân";
            public string Shift { get; set; } = "Sáng";
            public DateTime HireDate { get; set; } = DateTime.Now;
            public bool IsAdmin { get; set; }
        }
    }
}
