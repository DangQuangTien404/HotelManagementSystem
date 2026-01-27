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
        Task UpdateTaskAsync(int cleaningId, string status, int? staffId);
        Task DeleteCleaningAsync(int id);
        Task<RoomCleaningDto?> GetCleaningByIdAsync(int id);
        Task CreatePendingCleaningAsync(int roomId);
    }
}
