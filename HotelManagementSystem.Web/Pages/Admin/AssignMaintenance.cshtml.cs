using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelManagementSystem.Business;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignMaintenanceModel : PageModel
    {
        private readonly IMaintenanceService _service;

        public AssignMaintenanceModel(IMaintenanceService service)
        {
            _service = service;
        }

        [BindProperty]
        public MaintenanceTask MaintenanceTask { get; set; } = new();

        public SelectList RoomList { get; set; }
        public SelectList TechStaffList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            var rooms = await _service.GetRoomsNotInMaintenance();
            RoomList = new SelectList(rooms, "Id", "RoomNumber", roomId);

            var staffUsers = await _service.GetAssignableStaffUsers();
            TechStaffList = new SelectList(staffUsers, "Id", "FullName");

            if (roomId.HasValue)
            {
                MaintenanceTask.RoomId = roomId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var rooms = await _service.GetRoomsNotInMaintenance();
                RoomList = new SelectList(rooms, "Id", "RoomNumber");
                var techs = await _service.GetAssignableStaffUsers();
                TechStaffList = new SelectList(techs, "Id", "FullName");
                return Page();
            }

            await _service.AssignMaintenanceTask(MaintenanceTask);
            TempData["Message"] = "Đã phân công bảo trì cho phòng.";

            return RedirectToPage("/Index");
        }
    }
}
