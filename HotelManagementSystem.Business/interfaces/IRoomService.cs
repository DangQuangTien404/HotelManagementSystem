using HotelManagementSystem.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetAllRooms();
        Task<List<Room>> GetAvailableRoomsAsync(string? search, string? type);
        Task<List<string>> GetRoomTypesAsync();
        Task SaveRoomAsync(Room room);
    }
}
