using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class CleaningService
    {
        private readonly HotelManagementDbContext _context;

        public CleaningService(HotelManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomCleaning>> GetCleaningTasksForUserAsync(int userId)
        {
            return await _context.RoomCleanings
                .Include(c => c.Room)
                .Where(c => c.CleanedBy == userId && c.Status == "In Progress")
                .OrderBy(c => c.CleaningDate)
                .ToListAsync();
        }

        public async Task<bool> CompleteCleaningTaskAsync(int taskId, int userId)
        {
            var task = await _context.RoomCleanings
                .Include(t => t.Room)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CleanedBy == userId && t.Status == "In Progress");

            if (task == null)
            {
                return false;
            }

            task.Status = "Completed";
            if (task.Room != null)
            {
                task.Room.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
