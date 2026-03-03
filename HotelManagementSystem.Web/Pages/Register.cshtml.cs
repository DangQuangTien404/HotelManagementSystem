using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Business;

namespace HotelManagementSystem.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AccountService _accountService;

        public RegisterModel(HotelManagementDbContext context)
        {
            _accountService = new AccountService(context);
        }

        [BindProperty]
        public Customer Customer { get; set; } = new();

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

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

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập và mật khẩu là bắt buộc.");
                return Page();
            }

            try
            {
                // Xử lý chuỗi rỗng để tránh lỗi null! trong DB
                if (string.IsNullOrWhiteSpace(Customer.Address)) Customer.Address = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.IdentityNumber)) Customer.IdentityNumber = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.Email)) Customer.Email = "none@hotel.com";

                // Tạo User mới
                var newUser = new User
                {
                    Username = Username,
                    PasswordHash = Password, // Trong thực tế nên hash password
                    FullName = Customer.FullName,
                    Email = Customer.Email
                };

                // Đăng ký cả User và Customer
                var success = await _accountService.RegisterCustomer(newUser, Customer);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                    return Page();
                }

                // Đăng ký xong chuyển đến trang đăng nhập
                return RedirectToPage("/Login");
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