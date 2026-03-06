using HotelManagementSystem.Data.Models;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IStaffService
    {
        Task<Staff?> GetStaffByUserId(int userId);
        Task<bool> UpdateShift(int staffId, string newShift);
    }
}
