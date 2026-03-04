using HotelManagementSystem.Business;
using HotelManagementSystem.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HotelManagementSystem.Web.Services
{
    public class RoomUpdateBroadcaster : IRoomUpdateBroadcaster
    {
        private readonly IHubContext<RoomHub> _hubContext;

        public RoomUpdateBroadcaster(IHubContext<RoomHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastRoomStatusAsync(int roomId, string roomNumber, string newStatus)
        {
            await _hubContext.Clients.All.SendAsync("RoomStatusUpdated", new
            {
                id = roomId,
                roomNumber,
                status = newStatus
            });
        }
    }
}
