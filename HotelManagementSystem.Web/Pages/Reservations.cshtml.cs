using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        private readonly CheckInService _checkInService;
        private readonly CheckOutService _checkOutService;

        public List<Reservation> Reservations { get; set; } = new();

        public ReservationsModel(
            HotelManagementService hotelService,
            CheckInService checkInService,
            CheckOutService checkOutService)
        {
            _hotelService = hotelService;
            _checkInService = checkInService;
            _checkOutService = checkOutService;
        }

        public async Task OnGetAsync()
        {
            Reservations = await _hotelService.GetAdminActiveReservationsAsync();
        }

        public async Task<IActionResult> OnPostCheckInAsync(int id)
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim)) return RedirectToPage("/Login");

            int staffId = int.Parse(staffIdClaim);
            var success = await _checkInService.ExecuteCheckIn(id, staffId);

            if (success) TempData["Message"] = "Check-in thành công!";
            else TempData["Error"] = "Không thể thực hiện Check-in.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckOutAsync(int id)
        {
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim)) return RedirectToPage("/Login");

            int staffId = int.Parse(staffIdClaim);
            var success = await _checkOutService.ExecuteCheckOut(id, staffId);

            if (success) TempData["Message"] = "Check-out và thanh toán thành công!";
            else TempData["Error"] = "Lỗi trong quá trình thanh toán.";

            return RedirectToPage();
        }
    }
}
