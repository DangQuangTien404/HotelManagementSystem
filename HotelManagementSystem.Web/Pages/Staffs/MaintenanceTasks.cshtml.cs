using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Staff,Admin,Technician")]
    public class MaintenanceTasksModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public MaintenanceTasksModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        public List<MaintenanceTask> MyTasks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdValue, out int userId))
            {
                MyTasks = await _hotelService.GetMaintenanceTasksByUserAsync(userId);
            }
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out int userId)) return RedirectToPage("/Login");

            var updated = await _hotelService.UpdateMaintenanceTaskStatusAsync(id, userId);
            if (!updated) TempData["Error"] = "Bạn chỉ có thể hoàn thành công việc được phân công cho chính mình.";
            else TempData["Message"] = "Đã hoàn thành công việc bảo trì.";

            return RedirectToPage();
        }
    }
}
