using HotelManagementSystem.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public IndexModel(HotelManagementService hotelService) => _hotelService = hotelService;

        public List<RoomViewModel> RoomDisplay { get; set; } = new();

        public async Task OnGetAsync()
        {
            var rooms = await _hotelService.GetAllRoomsAsync();
            var activeReservations = await _hotelService.GetActiveReservationByRoomAsync();

            RoomDisplay = rooms.Select(r => new RoomViewModel
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Price = r.BasePrice,
                Status = r.Status,
                CurrentReservationId = activeReservations.TryGetValue(r.Id, out var reservationId) ? reservationId : null
            }).ToList();
        }

        public class RoomViewModel
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = null!;
            public string RoomType { get; set; } = null!;
            public decimal Price { get; set; }
            public string Status { get; set; } = null!;
            public int? CurrentReservationId { get; set; }
        }
    }
}
