using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

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

            try
            {
                // Gán các giá trị bắt buộc theo Model Customer.cs của bạn
                Customer.CreatedAt = DateTime.Now;

                // Xử lý chuỗi rỗng để tránh lỗi null! trong DB
                if (string.IsNullOrWhiteSpace(Customer.Address)) Customer.Address = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.IdentityNumber)) Customer.IdentityNumber = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.Email)) Customer.Email = "none@hotel.com";

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