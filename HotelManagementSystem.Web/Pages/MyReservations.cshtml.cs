using HotelManagementSystem.Business.service;
using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class MyReservationsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IStripeService _stripeService;
        private readonly HotelManagementDbContext _context;

        public List<Reservation> Reservations { get; set; } = new();

        public MyReservationsModel(
            IBookingService bookingService,
            IStripeService stripeService,
            HotelManagementDbContext context)
        {
            _bookingService = bookingService;
            _stripeService = stripeService;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return RedirectToPage("/Login");

            Reservations = await _bookingService.GetCustomerReservationsAsync(customer.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostRefundAsync(int reservationId)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return RedirectToPage("/Login");

            var (success, message) = await _bookingService.ProcessRefundAsync(
                reservationId, customer.Id, _stripeService);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToPage();
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public static DateTime GetRefundDeadline(Reservation reservation) =>
            reservation.CheckInDate.AddHours(-48);

        public static bool IsRefundEligible(Reservation reservation) =>
            DateTime.Now < GetRefundDeadline(reservation);
    }
}
