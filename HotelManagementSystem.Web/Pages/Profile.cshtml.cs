using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public ProfileModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
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

            UserProfile = await _hotelService.GetUserByIdAsync(int.Parse(userIdStr));
            if (UserProfile == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateInfoAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Login");

            var userToUpdate = await _hotelService.GetUserByIdAsync(int.Parse(userIdStr));
            if (userToUpdate != null)
            {
                userToUpdate.FullName = UserProfile.FullName;
                userToUpdate.Email = UserProfile.Email;
                await _hotelService.UpdateUserProfileAsync(userToUpdate);
                TempData["Message"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Login");
            var user = await _hotelService.GetUserByIdAsync(int.Parse(userIdStr));

            if (user != null)
            {
                if (user.PasswordHash != PasswordInput.OldPassword)
                {
                    ModelState.AddModelError("PasswordInput.OldPassword", "Mật khẩu cũ không chính xác");
                    return Page();
                }

                user.PasswordHash = PasswordInput.NewPassword;
                await _hotelService.UpdateUserProfileAsync(user);
                TempData["Message"] = "Đổi mật khẩu thành công!";
            }

            return RedirectToPage();
        }
    }
}
