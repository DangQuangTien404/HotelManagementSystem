using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<User>> GetStaffUsersAsync()
        {
            // Select Users where Id is present in the Staffs table (userId)
            // or where the user has entries in the Staffs navigation collection.
            return await _context.Users
                .Where(u => u.Staffs.Any())
                .ToListAsync();
        }
    }
}
