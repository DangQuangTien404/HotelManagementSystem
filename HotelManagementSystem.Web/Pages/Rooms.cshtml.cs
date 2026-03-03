using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class RoomsModel : PageModel
    {
        private readonly RoomService _roomService;
        private readonly HotelManagementDbContext _context;

        public List<Room> AvailableRooms { get; set; } = new();
        public List<string> RoomTypes { get; set; } = new();
        public Dictionary<int, List<(DateTime CheckIn, DateTime CheckOut)>> BookedPeriods { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Type { get; set; }

        public RoomsModel(RoomService roomService, HotelManagementDbContext context)
        {
            _roomService = roomService;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            AvailableRooms = await _roomService.GetAvailableRoomsAsync(Search, Type);
            RoomTypes = await _roomService.GetRoomTypesAsync();

            var roomIds = AvailableRooms.Select(r => r.Id).ToList();

            var reservations = await _context.Reservations
                .Where(r => roomIds.Contains(r.RoomId)
                    && (r.Status == "Confirmed" || r.Status == "PendingPayment" || r.Status == "CheckedIn")
                    && r.CheckOutDate >= DateTime.Today)
                .OrderBy(r => r.CheckInDate)
                .Select(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate })
                .ToListAsync();

            BookedPeriods = reservations
                .GroupBy(r => r.RoomId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => (r.CheckInDate, r.CheckOutDate)).ToList());
        }
    }
}
