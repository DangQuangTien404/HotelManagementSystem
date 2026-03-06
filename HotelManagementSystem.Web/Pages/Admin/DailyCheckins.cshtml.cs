using HotelManagementSystem.Business.service;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DailyCheckinsModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        private readonly CheckInService _checkInService;

        [BindProperty(SupportsGet = true)]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoomTypeFilter { get; set; }

        public List<Reservation> Reservations { get; set; } = new();
        public List<string> RoomTypes { get; set; } = new();

        public int PendingCount { get; set; }
        public int CheckedInCount { get; set; }
        public int NoShowCount { get; set; }
        public int AutoMarkedCount { get; set; }

        public DailyCheckinsModel(HotelManagementDbContext context, CheckInService checkInService)
        {
            _context = context;
            _checkInService = checkInService;
        }

        public async Task OnGetAsync()
        {
            AutoMarkedCount = await SweepNoShowsAsync();
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostCheckInAsync(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int staffId))
                return RedirectToPage("/Login");

            var success = await _checkInService.ExecuteCheckIn(id, staffId);

            TempData[success ? "Message" : "Error"] = success
                ? "Check-in thành công!"
                : "Không thể thực hiện check-in. Đặt phòng có thể đã được xử lý hoặc không hợp lệ.";

            return RedirectToPage(new { Date = Date.ToString("yyyy-MM-dd"), Search, StatusFilter, RoomTypeFilter });
        }

        public async Task<IActionResult> OnPostMarkNoShowAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == "Confirmed");

            if (reservation != null)
            {
                reservation.Status = "NoShow";
                if (reservation.Room != null)
                    reservation.Room.Status = "Available";
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Đã đánh dấu vắng mặt (No-show) cho đặt phòng #{id}.";
            }
            else
            {
                TempData["Error"] = "Không thể đánh dấu. Đặt phòng không ở trạng thái chờ check-in.";
            }

            return RedirectToPage(new { Date = Date.ToString("yyyy-MM-dd"), Search, StatusFilter, RoomTypeFilter });
        }

        // Sweep: mark Confirmed reservations as NoShow when it is past 14:00 the day after CheckInDate.
        private async Task<int> SweepNoShowsAsync()
        {
            var yesterday = DateTime.Today.AddDays(-1);

            // Pull candidates: Confirmed reservations whose check-in date is before today
            // (the deadline of CheckInDate+1day@14:00 can only pass for past dates)
            var candidates = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.Status == "Confirmed" && r.CheckInDate.Date <= yesterday)
                .ToListAsync();

            var now = DateTime.Now;
            var noShows = candidates
                .Where(r => now >= r.CheckInDate.Date.AddDays(1).AddHours(14))
                .ToList();

            foreach (var r in noShows)
            {
                r.Status = "NoShow";
                if (r.Room != null)
                    r.Room.Status = "Available";
            }

            if (noShows.Count > 0)
                await _context.SaveChangesAsync();

            return noShows.Count;
        }

        private async Task LoadAsync()
        {
            // Distinct room types for the filter dropdown
            RoomTypes = await _context.Rooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            // Summary counts for the selected day (unaffected by filters)
            var allStatuses = await _context.Reservations
                .Where(r => r.CheckInDate.Date == Date.Date)
                .Select(r => r.Status)
                .ToListAsync();

            PendingCount = allStatuses.Count(s => s == "Confirmed");
            CheckedInCount = allStatuses.Count(s => s == "CheckedIn");
            NoShowCount = allStatuses.Count(s => s == "NoShow");

            // Filtered list
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Include(r => r.CheckInOuts)
                .Where(r => r.CheckInDate.Date == Date.Date);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var term = Search.Trim().ToLower();
                query = query.Where(r =>
                    r.Customer.FullName.ToLower().Contains(term) ||
                    r.Room.RoomNumber.ToLower().Contains(term) ||
                    r.Customer.Phone.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
                query = query.Where(r => r.Status == StatusFilter);

            if (!string.IsNullOrWhiteSpace(RoomTypeFilter) && RoomTypeFilter != "All")
                query = query.Where(r => r.Room.RoomType == RoomTypeFilter);

            Reservations = await query.OrderBy(r => r.Room.RoomNumber).ToListAsync();
        }
    }
}
