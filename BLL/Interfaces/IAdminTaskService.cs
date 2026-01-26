using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminTaskService
    {
        Task<IEnumerable<MaintenanceTaskDto>> GetAllMaintenanceTasksAsync();
        Task<MaintenanceTaskDto?> GetMaintenanceTaskByIdAsync(int id);
        Task AssignMaintenanceTaskAsync(MaintenanceTaskDto taskDto);
        Task UpdateMaintenanceTaskAsync(MaintenanceTaskDto taskDto);
        Task UpdateTaskStatusAsync(int taskId, string status);
        Task ApproveMaintenanceTaskAsync(int taskId, int adminId);
    }
}

