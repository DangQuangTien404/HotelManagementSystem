using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using System;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Pages.Admin
{
    public class AddStaffModel : PageModel
    {
        private readonly AccountService _accountService;

        public AddStaffModel(AccountService accountService)
        {
            _accountService = accountService;
        }

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
                // CreatedAt will be set by AccountService
            };

            bool success = await _accountService.RegisterStaff(user, Input.Position, Input.Shift, Input.HireDate);

            if (success)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError("", "Thêm nhân viên thất bại. Có thể tên đăng nhập đã tồn tại hoặc có lỗi hệ thống.");
                return Page();
            }
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
