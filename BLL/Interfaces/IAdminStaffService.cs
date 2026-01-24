using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminStaffService
    {
        Task<IEnumerable<StaffDto>> GetAllStaffAsync();
        Task<StaffDto?> GetStaffByIdAsync(int id);
        Task AddStaffAsync(StaffDto staffDto);
        Task UpdateStaffAsync(StaffDto staffDto);
        Task DeleteStaffAsync(int id);
    }
}

