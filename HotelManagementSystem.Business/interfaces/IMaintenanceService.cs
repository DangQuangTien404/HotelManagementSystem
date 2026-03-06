using HotelManagementSystem.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IMaintenanceService
    {
        Task<List<Staff>> GetTechnicalStaff();
        Task<bool> CreateMaintenanceTask(int roomId, int staffUserId, string description, string priority, int creatorId);
    }
}
