using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AddStaffModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public AddStaffModel(HotelManagementDbContext context) => _context = context;

        [BindProperty]
        public StaffInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Sử dụng Transaction để đảm bảo tạo đủ cả User và Staff
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo đối tượng User
                var user = new User
                {
                    Username = Input.UserName,
                    // THAY 'PasswordHash' BẰNG TÊN CỘT TRONG FILE User.cs CỦA BẠN
                    PasswordHash = Input.Password,
                    Email = Input.Email,
                    Role = Input.IsAdmin ? "Admin" : "Staff",
                    FullName = Input.FullName,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Lưu để lấy Id của User

                // 2. Tạo đối tượng Staff liên kết với User vừa tạo
                var staff = new HotelManagementSystem.Models.Staff
                {
                    UserId = user.Id, // Lấy ID từ User vừa tạo ở trên
                    Position = Input.Position,
                    Shift = Input.Shift,
                    HireDate = Input.HireDate,
                    // Đảm bảo không truyền User object nếu DB đã tự handle
                };

                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Hiển thị lỗi ra màn hình để bạn dễ debug
                ModelState.AddModelError("", "Lỗi DB: " + ex.Message);
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
