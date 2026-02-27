using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public CreateModel(HotelManagementService hotelService) => _hotelService = hotelService;

        [BindProperty] public Reservation Reservation { get; set; } = new();

        public Room? SelectedRoom { get; set; }
        public List<Customer> CustomerList { get; set; } = new();
        public List<HotelService> ActiveServices { get; set; } = new();

        [BindProperty] public List<int> SelectedServiceIds { get; set; } = new();

        public async Task OnGetAsync(int? roomId)
        {
            CustomerList = await _hotelService.GetCustomersAsync();
            ActiveServices = await _hotelService.GetActiveServicesAsync();

            if (roomId.HasValue)
            {
                SelectedRoom = await _hotelService.GetRoomAsync(roomId.Value);
                if (SelectedRoom != null) Reservation.RoomId = SelectedRoom.Id;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CustomerList = await _hotelService.GetCustomersAsync();
            ActiveServices = await _hotelService.GetActiveServicesAsync();

            Reservation.Status = "Booked";
            Reservation.CreatedAt = DateTime.Now;

            await _hotelService.CreateReservationAsync(Reservation, SelectedServiceIds);
            return RedirectToPage("/Index");
        }
    }
}
