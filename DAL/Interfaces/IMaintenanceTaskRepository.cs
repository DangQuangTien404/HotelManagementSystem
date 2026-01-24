using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IMaintenanceTaskRepository : IGenericRepository<MaintenanceTask>
    {
        Task<IEnumerable<MaintenanceTask>> GetAllTasksWithDetailsAsync();
        Task<MaintenanceTask?> GetTaskWithDetailsByIdAsync(int id);
    }
}

