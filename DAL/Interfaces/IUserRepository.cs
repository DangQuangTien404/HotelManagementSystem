using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<IEnumerable<User>> GetStaffUsersAsync();
        Task<IEnumerable<User>> GetAllWithDetailsAsync();
        Task<User?> GetByUsernameAsync(string username);
    }
}
