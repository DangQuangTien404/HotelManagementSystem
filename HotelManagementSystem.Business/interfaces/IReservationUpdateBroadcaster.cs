namespace HotelManagementSystem.Business.interfaces
{
    public interface IReservationUpdateBroadcaster
    {
        Task BroadcastReservationCheckInAsync(int reservationId, int roomId, string roomNumber, string customerName);
        Task BroadcastReservationCheckOutAsync(int reservationId, int roomId, string roomNumber, string customerName);
        Task BroadcastReservationStatusChangedAsync(int reservationId, string oldStatus, string newStatus);
        Task BroadcastReservationCreatedAsync(int reservationId, int roomId, string roomNumber, string customerName, DateTime checkInDate, DateTime checkOutDate);
    }
}
