using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public ProfileModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User UserProfile { get; set; } = default!;

        [BindProperty]
        public ChangePasswordViewModel PasswordInput { get; set; } = new();

        public class ChangePasswordViewModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
            [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Login");

            UserProfile = await _context.Users.FindAsync(int.Parse(userIdStr));

            if (UserProfile == null) return NotFound();
            return Page();
        }

        // Xử lý cập nhật thông tin cơ bản
        public async Task<IActionResult> OnPostUpdateInfoAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userToUpdate = await _context.Users.FindAsync(int.Parse(userIdStr));

            if (userToUpdate != null)
            {
                userToUpdate.FullName = UserProfile.FullName;
                userToUpdate.Email = UserProfile.Email;

                await _context.SaveChangesAsync();
                TempData["Message"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToPage();
        }

        // Xử lý đổi mật khẩu
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userIdStr));

            if (user != null)
            {
                // Trong thực tế, bạn nên dùng BCrypt hoặc Identity để Verify mật khẩu
                if (user.PasswordHash != PasswordInput.OldPassword)
                {
                    ModelState.AddModelError("PasswordInput.OldPassword", "Mật khẩu cũ không chính xác");
                    return Page();
                }

                user.PasswordHash = PasswordInput.NewPassword;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đổi mật khẩu thành công!";
            }

            return RedirectToPage();
        }
    }
}