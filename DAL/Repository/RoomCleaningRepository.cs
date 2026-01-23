using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class RoomCleaningRepository : GenericRepository<RoomCleaning>, IRoomCleaningRepository
    {
        public RoomCleaningRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RoomCleaning>> GetPendingCleaningsAsync()
        {
            return await _context.RoomCleanings
                .Include(rc => rc.Room)
                .Include(rc => rc.Cleaner)
                .Where(rc => rc.Status != "Completed")
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomCleaning>> GetAllWithDetailsAsync()
        {
            return await _context.RoomCleanings
                .Include(rc => rc.Room)
                .Include(rc => rc.Cleaner)
                .ToListAsync();
        }

        public async Task<RoomCleaning?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.RoomCleanings
                .Include(rc => rc.Room)
                .Include(rc => rc.Cleaner)
                .FirstOrDefaultAsync(rc => rc.Id == id);
        }
    }
}
