using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRoomCleaningService
    {
        Task<IEnumerable<RoomCleaningDto>> GetAllCleaningsAsync();
        Task<IEnumerable<RoomCleaningDto>> GetPendingCleaningsAsync();
        Task AssignCleanerAsync(int roomId, int staffUserId);
        Task UpdateStatusAsync(int cleaningId, string status);
        Task DeleteCleaningAsync(int id);
        Task<RoomCleaningDto?> GetCleaningByIdAsync(int id);
    }
}
