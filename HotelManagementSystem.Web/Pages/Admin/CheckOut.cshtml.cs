using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CheckOutModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public CheckOutModel(HotelManagementService hotelService) => _hotelService = hotelService;

        [BindProperty]
        public Reservation Reservation { get; set; } = null!;
        public int TotalDays { get; set; }
        public decimal RoomTotalPrice { get; set; }
        public decimal ServiceTotalPrice { get; set; }
        public decimal TotalPrice => RoomTotalPrice + ServiceTotalPrice;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _hotelService.GetCheckoutReservationAsync(id);
            if (Reservation == null) return RedirectToPage("/Index");

            var diff = DateTime.Now - Reservation.CheckInDate;
            TotalDays = diff.Days <= 0 ? 1 : diff.Days;
            RoomTotalPrice = TotalDays * (Reservation.Room?.BasePrice ?? 0);
            ServiceTotalPrice = await _hotelService.GetServiceTotalPriceAsync(Reservation.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _hotelService.CheckoutAsync(id);
            return RedirectToPage("/Index");
        }
    }
}
