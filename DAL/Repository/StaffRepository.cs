using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Staff>> GetAllStaffWithUserAsync()
        {
            return await _context.Staffs
                .Include(s => s.User)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();
        }

        public async Task<Staff?> GetStaffWithUserByIdAsync(int id)
        {
            return await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Staff?> GetStaffByUserIdAsync(int userId)
        {
            return await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}

