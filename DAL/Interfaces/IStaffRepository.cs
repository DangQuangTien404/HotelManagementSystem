using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Task<IEnumerable<Staff>> GetAllStaffWithUserAsync();
        Task<Staff?> GetStaffWithUserByIdAsync(int id);
        Task<Staff?> GetStaffByUserIdAsync(int userId);
    }
}

