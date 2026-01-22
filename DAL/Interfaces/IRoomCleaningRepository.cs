using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRoomCleaningRepository : IGenericRepository<RoomCleaning>
    {
        Task<IEnumerable<RoomCleaning>> GetPendingCleaningsAsync();
        Task<IEnumerable<RoomCleaning>> GetAllWithDetailsAsync();
        Task<RoomCleaning?> GetByIdWithDetailsAsync(int id);
    }
}
