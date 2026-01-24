using DTOs;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IStaffDashboardService
    {
        Task<StaffDashboardDto> GetDashboardDataAsync();
        Task<StaffTaskDto> GetStaffTasksAsync(int staffUserId);
    }
}
