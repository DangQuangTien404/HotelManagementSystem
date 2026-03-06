using HotelManagementSystem.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface ICleaningService
    {
        Task<List<RoomCleaning>> GetCleaningTasksForUserAsync(int userId);
        Task<bool> CompleteCleaningTaskAsync(int taskId, int userId);
    }
}
