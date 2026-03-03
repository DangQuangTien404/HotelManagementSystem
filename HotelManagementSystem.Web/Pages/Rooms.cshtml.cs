using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class RoomsModel : PageModel
    {
        private readonly RoomService _roomService;

        public List<Room> AvailableRooms { get; set; } = new();
        public List<string> RoomTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        public RoomsModel(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task OnGetAsync()
        {
            AvailableRooms = await _roomService.GetAvailableRoomsAsync(Search, Type);
            RoomTypes = await _roomService.GetRoomTypesAsync();
        }
    }
}
