using DAL;
using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class MaintenanceTaskRepository : GenericRepository<MaintenanceTask>, IMaintenanceTaskRepository
    {
        public MaintenanceTaskRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceTask>> GetAllTasksWithDetailsAsync()
        {
            return await _context.MaintenanceTasks
                .Include(mt => mt.Room)
                .Include(mt => mt.AssignedStaff)
                .Include(mt => mt.ApprovedByUser)
                .OrderByDescending(mt => mt.CreatedAt)
                .ToListAsync();
        }

        public async Task<MaintenanceTask?> GetTaskWithDetailsByIdAsync(int id)
        {
            return await _context.MaintenanceTasks
                .Include(mt => mt.Room)
                .Include(mt => mt.AssignedStaff)
                .Include(mt => mt.ApprovedByUser)
                .FirstOrDefaultAsync(mt => mt.Id == id);
        }
    }
}

