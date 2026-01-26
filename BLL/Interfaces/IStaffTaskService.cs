using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IStaffTaskService
    {
        /// <summary>
        /// Lấy tất cả tasks được giao cho staff cụ thể
        /// </summary>
        Task<IEnumerable<MaintenanceTaskDto>> GetTasksByStaffIdAsync(int staffUserId);

        /// <summary>
        /// Lấy task theo ID (chỉ nếu task được giao cho staff này)
        /// </summary>
        Task<MaintenanceTaskDto?> GetTaskByIdForStaffAsync(int taskId, int staffUserId);

        /// <summary>
        /// Staff update status của task được giao cho mình
        /// </summary>
        Task UpdateMyTaskStatusAsync(int taskId, int staffUserId, string status);

        /// <summary>
        /// Staff đánh dấu task là hoàn thành
        /// </summary>
        Task CompleteMyTaskAsync(int taskId, int staffUserId);
    }
}
