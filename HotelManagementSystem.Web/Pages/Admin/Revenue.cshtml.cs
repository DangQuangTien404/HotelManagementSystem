using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RevenueModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public RevenueModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int TotalBookings { get; set; }
        public List<Reservation> RecentTransactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var completedReservations = await _hotelService.GetCompletedReservationsInRangeAsync(DateTime.MinValue, DateTime.MaxValue);

            RecentTransactions = completedReservations.Take(10).ToList();
            TotalBookings = completedReservations.Count;

            RoomRevenue = 0;
            foreach (var res in completedReservations)
            {
                var days = (res.CheckOutDate - res.CheckInDate).Days;
                if (days <= 0) days = 1;
                RoomRevenue += days * (res.Room?.BasePrice ?? 0);
            }

            ServiceRevenue = await _hotelService.GetServiceRevenueInRangeAsync(DateTime.MinValue, DateTime.MaxValue);
            TotalRevenue = RoomRevenue + ServiceRevenue;
        }
    }
}
