using DTOs;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
    }
}

