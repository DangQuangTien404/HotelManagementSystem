using HotelManagementSystem.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Staff,Admin")]
    public class MyTasksModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public MyTasksModel(HotelManagementDbContext context) => _context = context;

        public List<HotelManagementSystem.Models.RoomCleaning> MyCleaningTasks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            MyCleaningTasks = await _context.RoomCleanings
                .Include(c => c.Room)
                .Where(c => c.CleanedBy == userId.Value && c.Status == "In Progress")
                .OrderBy(c => c.CleaningDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int taskId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            var task = await _context.RoomCleanings
                .Include(t => t.Room)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CleanedBy == userId.Value && t.Status == "In Progress");

            if (task == null)
            {
                TempData["Error"] = "Không tìm thấy công việc hợp lệ để cập nhật.";
                return RedirectToPage();
            }

            task.Status = "Completed";
            if (task.Room != null)
            {
                task.Room.Status = "Available";
            }

            await _context.SaveChangesAsync();
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
