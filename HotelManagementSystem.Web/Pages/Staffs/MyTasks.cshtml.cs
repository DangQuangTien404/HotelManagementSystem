using HotelManagementSystem.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Staff,Admin")]
    public class MyTasksModel : PageModel
    {
        private readonly CleaningService _cleaningService;

        public MyTasksModel(CleaningService cleaningService)
        {
            _cleaningService = cleaningService;
        }

        public List<HotelManagementSystem.Data.Models.RoomCleaning> MyCleaningTasks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            MyCleaningTasks = await _cleaningService.GetCleaningTasksForUserAsync(userId.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int taskId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var success = await _cleaningService.CompleteCleaningTaskAsync(taskId, userId.Value);

            if (!success)
            {
                TempData["Error"] = "Không tìm thấy công việc hợp lệ để cập nhật.";
                return RedirectToPage();
            }

            TempData["Message"] = "Đã xác nhận hoàn thành công việc.";
            return RedirectToPage();
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}
