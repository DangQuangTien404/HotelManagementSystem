using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RoomsModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public RoomsModel(HotelManagementService hotelService) => _hotelService = hotelService;

        public IList<Room> Rooms { get; set; } = default!;

        [BindProperty]
        public Room NewRoom { get; set; } = new();

        public async Task OnGetAsync() => Rooms = await _hotelService.GetAllRoomsAsync();

        public async Task<IActionResult> OnPostSaveRoomAsync()
        {
            await _hotelService.SaveRoomAsync(NewRoom);
            return RedirectToPage();
        }
    }
}
