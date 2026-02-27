using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Business;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OperationsModel : PageModel
    {
        private readonly RoomService _roomService;
        private readonly IMaintenanceService _maintenanceService;

        public OperationsModel(RoomService roomService, IMaintenanceService maintenanceService)
        {
            _roomService = roomService;
            _maintenanceService = maintenanceService;
        }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int OccupiedRooms { get; set; }

        public List<MaintenanceTask> PendingTasks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allRooms = await _roomService.GetAllRooms();
            TotalRooms = allRooms.Count;
            AvailableRooms = allRooms.Count(r => r.Status == "Available");
            MaintenanceRooms = allRooms.Count(r => r.Status == "Maintenance");
            OccupiedRooms = allRooms.Count(r => r.Status == "Occupied");

            PendingTasks = await _maintenanceService.GetOpenTasks();
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int id)
        {
            var task = await _maintenanceService.CompleteTaskByAdmin(id);

            if (task != null)
            {
                TempData["Message"] = $"Đã duyệt hoàn tất sửa chữa phòng {task.Room?.RoomNumber}";
            }

            return RedirectToPage();
        }
    }
}
