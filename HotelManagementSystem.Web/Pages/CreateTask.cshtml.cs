using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Maintenance
{
    public class CreateTaskModel : PageModel
    {
        private readonly MaintenanceService _service;
        private readonly RoomService _roomService;

        public CreateTaskModel(MaintenanceService service, RoomService roomService)
        {
            _service = service;
            _roomService = roomService;
        }

        [BindProperty] public int SelectedRoomId { get; set; }
        [BindProperty] public int SelectedStaffUserId { get; set; }
        [BindProperty] public string Description { get; set; }
        [BindProperty] public string Priority { get; set; }

        public SelectList RoomList { get; set; }
        public SelectList StaffList { get; set; }

        public async Task OnGetAsync()
        {
            var rooms = await _roomService.GetAllRooms();
            var staff = await _service.GetTechnicalStaff();

            RoomList = new SelectList(rooms, "Id", "RoomNumber");
            // Lưu ý: Staff trỏ về UserId để nhận Notification
            StaffList = new SelectList(staff.Select(s => s.User), "Id", "FullName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var creatorIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(creatorIdValue, out var creatorId))
            {
                return RedirectToPage("/Login");
            }

            var success = await _service.CreateMaintenanceTask(
                SelectedRoomId, SelectedStaffUserId, Description, Priority, creatorId);

            if (success) return RedirectToPage("/Index");

            return Page();
        }
    }
}