using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Consumer")]
    public class CheckOutModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        private readonly CheckOutService _checkOutService;

        public CheckOutModel(HotelManagementDbContext context, CheckOutService checkOutService)
        {
            _context = context;
            _checkOutService = checkOutService;
        }

        [BindProperty]
        public Reservation Reservation { get; set; } = null!;

        public int TotalDays { get; set; }
        public decimal RoomTotalPrice { get; set; }
        public decimal ServiceTotalPrice { get; set; }
        public decimal TotalPrice => RoomTotalPrice + ServiceTotalPrice;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var reservation = await GetCustomerReservationAsync(id);
            if (reservation == null) return RedirectToPage("/MyBookings");

            Reservation = reservation;
            await CalculateTotalsAsync(reservation);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var reservation = await GetCustomerReservationAsync(id);
            if (reservation == null) return RedirectToPage("/MyBookings");

            var success = await _checkOutService.ExecuteCheckOut(id);
            if (!success)
            {
                TempData["Error"] = "Không thể thực hiện check-out. Vui lòng thử lại.";
                return RedirectToPage("/MyBookings");
            }

            TempData["Message"] = "Check-out thành công. Cảm ơn bạn đã sử dụng dịch vụ!";
            return RedirectToPage("/MyBookings");
        }

        private async Task<Reservation?> GetCustomerReservationAsync(int reservationId)
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerIdClaim)) return null;

            var customerId = int.Parse(customerIdClaim);

            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.CustomerId == customerId && r.Status == "CheckedIn");
        }

        private async Task CalculateTotalsAsync(Reservation reservation)
        {
            var checkInTime = await _context.CheckInOuts
                .Where(c => c.ReservationId == reservation.Id)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => c.CheckInTime)
                .FirstOrDefaultAsync();

            var startDate = checkInTime ?? reservation.CheckInDate;
            var diff = DateTime.Now - startDate;
            TotalDays = diff.Days <= 0 ? 1 : diff.Days;

            RoomTotalPrice = TotalDays * (reservation.Room?.Price ?? 0);
            ServiceTotalPrice = await _context.ReservationServices
                .Where(s => s.ReservationId == reservation.Id)
                .SumAsync(s => s.Quantity * s.UnitPrice);
        }
    }
}
