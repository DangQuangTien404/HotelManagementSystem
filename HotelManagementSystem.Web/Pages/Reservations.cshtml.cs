using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Business;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        private readonly CheckInService _checkInService;
        private readonly CheckOutService _checkOutService;

        public List<Reservation> Reservations { get; set; } = new();

        public ReservationsModel(
            HotelManagementDbContext context,
            CheckInService checkInService,
            CheckOutService checkOutService)
        {
            _context = context;
            _checkInService = checkInService;
            _checkOutService = checkOutService;
        }

        public async Task OnGetAsync()
        {
            Reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Where(r => r.Status == "Confirmed" || r.Status == "CheckedIn")
                .OrderBy(r => r.CheckInDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCheckInAsync(int id)
        {
            // Lấy ID của nhân viên đang thao tác từ Cookie
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim)) return RedirectToPage("/Login");

            int staffId = int.Parse(staffIdClaim);

            // Thực hiện Check-in với ID nhân viên
            var success = await _checkInService.ExecuteCheckIn(id, staffId);

            if (success) TempData["Message"] = "Check-in thành công!";
            else TempData["Error"] = "Không thể thực hiện Check-in.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckOutAsync(int id)
        {
            // Lấy ID của nhân viên đang thao tác
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim)) return RedirectToPage("/Login");

            int staffId = int.Parse(staffIdClaim);

            // Thực hiện Check-out với ID nhân viên
            var success = await _checkOutService.ExecuteCheckOut(id, staffId);

            if (success) TempData["Message"] = "Check-out và thanh toán thành công!";
            else TempData["Error"] = "Lỗi trong quá trình thanh toán.";

            return RedirectToPage();
        }
    }
}