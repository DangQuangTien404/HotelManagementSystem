using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HotelManagementSystem.Web.Services
{
    public class ReservationUpdateBroadcaster : IReservationUpdateBroadcaster
    {
        private readonly IHubContext<ReservationHub> _hubContext;

        public ReservationUpdateBroadcaster(IHubContext<ReservationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastReservationCheckInAsync(int reservationId, int roomId, string roomNumber, string customerName)
        {
            await _hubContext.Clients.Group("ReservationAdmins").SendAsync("ReservationCheckedIn", new
            {
                reservationId,
                roomId,
                roomNumber,
                customerName,
                timestamp = DateTime.Now
            });
        }

        public async Task BroadcastReservationCheckOutAsync(int reservationId, int roomId, string roomNumber, string customerName)
        {
            await _hubContext.Clients.Group("ReservationAdmins").SendAsync("ReservationCheckedOut", new
            {
                reservationId,
                roomId,
                roomNumber,
                customerName,
                timestamp = DateTime.Now
            });
        }

        public async Task BroadcastReservationStatusChangedAsync(int reservationId, string oldStatus, string newStatus)
        {
            await _hubContext.Clients.Group("ReservationAdmins").SendAsync("ReservationStatusChanged", new
            {
                reservationId,
                oldStatus,
                newStatus,
                timestamp = DateTime.Now
            });
        }

        public async Task BroadcastReservationCreatedAsync(int reservationId, int roomId, string roomNumber, string customerName, DateTime checkInDate, DateTime checkOutDate)
        {
            await _hubContext.Clients.Group("ReservationAdmins").SendAsync("ReservationCreated", new
            {
                reservationId,
                roomId,
                roomNumber,
                customerName,
                checkInDate,
                checkOutDate,
                status = "Confirmed",
                timestamp = DateTime.Now
            });
        }
    }
}
