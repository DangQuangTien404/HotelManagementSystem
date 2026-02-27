using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OperationsModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public OperationsModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public List<MaintenanceTask> PendingTasks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var stats = await _hotelService.GetRoomStatsAsync();
            TotalRooms = stats.total;
            AvailableRooms = stats.available;
            MaintenanceRooms = stats.maintenance;
            OccupiedRooms = stats.occupied;
            PendingTasks = await _hotelService.GetPendingMaintenanceTasksAsync();
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int id)
        {
            var ok = await _hotelService.CompleteMaintenanceTaskAsync(id);
            if (ok) TempData["Message"] = "Đã duyệt hoàn tất sửa chữa.";
            return RedirectToPage();
        }
    }
}
