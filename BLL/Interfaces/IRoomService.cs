using DTOs;
using DTOs.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int id);
        Task AddRoomAsync(RoomDto roomDto);
        Task UpdateRoomAsync(RoomDto roomDto);
        Task DeleteRoomAsync(int id);
        Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(string? searchTerm, RoomType? roomType, decimal? maxPrice);
        Task UpdateRoomStatusAsync(int roomId, string status);
    }
}
