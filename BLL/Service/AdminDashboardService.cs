using BLL.Interfaces;
using DTOs;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AdminDashboardService : IAdminDashboardService
    {
        public AdminDashboardService()
        {
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            return await Task.FromResult(new AdminDashboardDto());
        }
    }
}

