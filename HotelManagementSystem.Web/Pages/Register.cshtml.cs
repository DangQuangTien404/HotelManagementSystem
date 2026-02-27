using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Business;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AccountService _accountService;

        public RegisterModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;
            [Required]
            public string Password { get; set; } = string.Empty;
            [Required]
            public string FullName { get; set; } = string.Empty;
            [Required]
            public string Phone { get; set; } = string.Empty;
            [Required]
            public string Email { get; set; } = string.Empty;
            [Required]
            public string IdentityNumber { get; set; } = string.Empty;
            [Required]
            public string Address { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var newUser = new User
                {
                    Username = Input.Username,
                    PasswordHash = Input.Password, // In real app, hash this!
                    FullName = Input.FullName,
                    Email = Input.Email,
                    Role = "Consumer"
                };

                var newCustomer = new Customer
                {
                    FullName = Input.FullName,
                    Phone = Input.Phone,
                    Email = Input.Email,
                    IdentityNumber = Input.IdentityNumber,
                    Address = Input.Address
                };

                var result = await _accountService.RegisterConsumer(newUser, newCustomer);

                if (result)
                {
                    return RedirectToPage("/Login", new { Message = "Đăng ký thành công! Vui lòng đăng nhập." });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập đã tồn tại hoặc lỗi hệ thống.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi: " + ex.Message);
                return Page();
            }
        }
    }
}
