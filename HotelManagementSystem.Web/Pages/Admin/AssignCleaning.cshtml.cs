using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignCleaningModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public AssignCleaningModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        public Room Room { get; set; } = default!;
        public List<Staff> StaffList { get; set; } = new();

        [BindProperty]
        public int SelectedStaffUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(int roomId)
        {
            Room = await _hotelService.GetRoomAsync(roomId);
            if (Room == null) return NotFound();

            StaffList = await _hotelService.GetStaffListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int roomId)
        {
            var ok = await _hotelService.AssignCleaningAsync(roomId, SelectedStaffUserId);
            if (!ok)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu.");
                Room = await _hotelService.GetRoomAsync(roomId);
                StaffList = await _hotelService.GetStaffListAsync();
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
