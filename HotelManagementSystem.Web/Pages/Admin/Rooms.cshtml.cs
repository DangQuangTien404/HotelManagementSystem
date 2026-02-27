using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RoomsModel : PageModel
    {
        private readonly RoomService _roomService;

        public RoomsModel(RoomService roomService)
        {
            _roomService = roomService;
        }

        public IList<Room> Rooms { get; set; } = default!;

        [BindProperty]
        public Room NewRoom { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _roomService.GetAllRooms();
        }

        // Thêm hoặc Cập nhật phòng
        public async Task<IActionResult> OnPostSaveRoomAsync()
        {
            if (!ModelState.IsValid)
            {
                // In case of validation errors, reload the room list and return the page
                Rooms = await _roomService.GetAllRooms();
                return Page();
            }

            await _roomService.SaveRoomAsync(NewRoom);
            return RedirectToPage();
        }
    }
}
