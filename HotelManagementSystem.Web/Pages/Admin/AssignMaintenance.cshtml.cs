using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignMaintenanceModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public AssignMaintenanceModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        [BindProperty]
        public MaintenanceTask MaintenanceTask { get; set; } = new();

        public SelectList RoomList { get; set; }
        public SelectList TechStaffList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            var rooms = (await _hotelService.GetAllRoomsAsync()).Where(r => r.Status != "Maintenance").ToList();
            RoomList = new SelectList(rooms, "Id", "RoomNumber", roomId);

            var staffUsers = await _hotelService.GetStaffUsersAsync();
            TechStaffList = new SelectList(staffUsers, "Id", "FullName");

            if (roomId.HasValue) MaintenanceTask.RoomId = roomId.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var rooms = await _hotelService.GetAllRoomsAsync();
                RoomList = new SelectList(rooms, "Id", "RoomNumber");
                var techs = await _hotelService.GetStaffUsersAsync();
                TechStaffList = new SelectList(techs, "Id", "FullName");
                return Page();
            }

            await _hotelService.AssignMaintenanceAsync(MaintenanceTask);
            TempData["Message"] = "Đã phân công bảo trì.";
            return RedirectToPage("/Index");
        }
    }
}
