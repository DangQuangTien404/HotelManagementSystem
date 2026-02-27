using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Business;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Staff,Admin,Technician")]
    public class MaintenanceTasksModel : PageModel
    {
        private readonly IMaintenanceService _service;

        public MaintenanceTasksModel(IMaintenanceService service)
        {
            _service = service;
        }

        public List<MaintenanceTask> MyTasks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdValue, out int userId))
            {
                MyTasks = await _service.GetPendingTasks(userId);
            }
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out int userId))
            {
                return RedirectToPage("/Login");
            }

            var task = await _service.CompleteTaskByStaff(id, userId);

            if (task == null)
            {
                TempData["Error"] = "Bạn chỉ có thể hoàn thành công việc được phân công cho chính mình.";
                return RedirectToPage();
            }

            TempData["Message"] = $"Đã hoàn thành sửa chữa phòng {task.Room?.RoomNumber}";

            return RedirectToPage();
        }
    }
}
