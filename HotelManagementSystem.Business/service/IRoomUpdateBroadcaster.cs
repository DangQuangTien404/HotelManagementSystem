namespace HotelManagementSystem.Business.service
{
    public interface IRoomUpdateBroadcaster
    {
        Task BroadcastRoomStatusAsync(int roomId, string roomNumber, string newStatus);
    }
}
