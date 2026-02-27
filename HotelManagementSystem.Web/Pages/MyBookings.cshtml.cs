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
    public class MyBookingsModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public MyBookingsModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        public List<Reservation> Reservations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerIdClaim)) return RedirectToPage("/Login");

            var customerId = int.Parse(customerIdClaim);

            Reservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCheckInAsync(int reservationId)
        {
            var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerIdClaim)) return RedirectToPage("/Login");

            var customerId = int.Parse(customerIdClaim);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.CustomerId == customerId);

            if (reservation == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng của bạn.";
                return RedirectToPage();
            }

            if (reservation.Status != "Confirmed")
            {
                TempData["Error"] = "Chỉ đặt phòng đã được xác nhận mới có thể check-in.";
                return RedirectToPage();
            }

            reservation.Status = "CheckedIn";
            _context.CheckInOuts.Add(new CheckInOut
            {
                ReservationId = reservation.Id,
                CheckInTime = DateTime.Now,
                CheckInBy = null,
                TotalAmount = 0
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = "Check-in thành công. Chúc bạn có kỳ nghỉ vui vẻ!";
            return RedirectToPage();
        }
    }
}
