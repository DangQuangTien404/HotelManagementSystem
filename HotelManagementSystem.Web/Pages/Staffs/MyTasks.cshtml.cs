using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Staff,Admin")]
    public class MyTasksModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public MyTasksModel(HotelManagementService hotelService) => _hotelService = hotelService;

        public List<RoomCleaning> MyCleaningTasks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue) MyCleaningTasks = await _hotelService.GetCleaningTasksByStaffAsync(userId.Value);
        }

        public async Task<IActionResult> OnPostCompleteAsync(int taskId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToPage("/Login");

            var ok = await _hotelService.CompleteCleaningTaskAsync(taskId, userId.Value);
            if (!ok) TempData["Error"] = "Không tìm thấy công việc hợp lệ để cập nhật.";
            else TempData["Message"] = "Đã xác nhận hoàn thành công việc.";
            return RedirectToPage();
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}
