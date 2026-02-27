using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Business;
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
        private readonly CheckInService _checkInService;

        public MyBookingsModel(HotelManagementDbContext context, CheckInService checkInService)
        {
            _context = context;
            _checkInService = checkInService;
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

            var success = await _checkInService.ExecuteCheckIn(reservation.Id);
            if (!success)
            {
                TempData["Error"] = "Không thể thực hiện check-in. Vui lòng thử lại.";
                return RedirectToPage();
            }

            TempData["Message"] = "Check-in thành công. Chúc bạn có kỳ nghỉ vui vẻ!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStartCheckOutAsync(int reservationId)
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

            if (reservation.Status != "CheckedIn")
            {
                TempData["Error"] = "Chỉ có thể trả phòng khi đang ở trạng thái Checked In.";
                return RedirectToPage();
            }

            return RedirectToPage("/CheckOut", new { id = reservation.Id });
        }
    }
}
