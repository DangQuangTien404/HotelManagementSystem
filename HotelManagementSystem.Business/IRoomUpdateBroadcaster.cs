namespace HotelManagementSystem.Business
{
    public interface IRoomUpdateBroadcaster
    {
        Task BroadcastRoomStatusAsync(int roomId, string roomNumber, string newStatus);
    }
}
