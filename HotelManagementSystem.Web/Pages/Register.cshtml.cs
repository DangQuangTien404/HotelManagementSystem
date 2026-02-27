using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public RegisterModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = new();

        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        public class RegisterInput
        {
            [Required]
            public string Username { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Reset form khi truy cập mới
            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra tính hợp lệ của dữ liệu
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existedUsername = await _context.Customers.AnyAsync(c => c.Username == Input.Username)
                                  || await _context.Users.AnyAsync(u => u.Username == Input.Username);
            if (existedUsername)
            {
                ModelState.AddModelError("Input.Username", "Tên đăng nhập đã tồn tại.");
                return Page();
            }

            try
            {
                // Gán các giá trị bắt buộc theo Model Customer.cs của bạn
                Customer.CreatedAt = DateTime.Now;

                // Xử lý chuỗi rỗng để tránh lỗi null! trong DB
                if (string.IsNullOrWhiteSpace(Customer.Address)) Customer.Address = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.IdentityNumber)) Customer.IdentityNumber = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.Email)) Customer.Email = "none@hotel.com";
                Customer.Username = Input.Username.Trim();
                Customer.PasswordHash = Input.Password;

                _context.Customers.Add(Customer);
                await _context.SaveChangesAsync();

                // Đăng ký xong quay về trang chủ
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi cụ thể nếu lưu thất bại
                ModelState.AddModelError(string.Empty, "Lỗi: " + ex.InnerException?.Message ?? ex.Message);
                return Page();
            }
        }
    }
}
