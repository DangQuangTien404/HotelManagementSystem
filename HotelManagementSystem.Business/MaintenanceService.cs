using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly HotelManagementDbContext _context;
        public MaintenanceService(HotelManagementDbContext context) => _context = context;

        public async Task<List<Staff>> GetTechnicalStaff()
        {
            return await _context.Staffs
                .Include(s => s.User)
                .Where(s => s.Position == "Technician" || s.Position == "Maintenance")
                .ToListAsync();
        }

        public async Task<List<MaintenanceTask>> GetPendingTasks(int staffId)
        {
            return await _context.MaintenanceTasks
                .Include(t => t.Room)
                .Include(t => t.Staff)
                .Where(t => t.AssignedTo == staffId && t.Status != "Completed")
                .OrderByDescending(t => t.Priority == "High")
                .ThenBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<List<MaintenanceTask>> GetOpenTasks()
        {
            return await _context.MaintenanceTasks
                .Include(m => m.Room)
                .Where(m => m.Status != "Completed")
                .OrderByDescending(m => m.Priority == "High")
                .ThenBy(m => m.Deadline)
                .ToListAsync();
        }

        public async Task<MaintenanceTask?> CompleteTaskByStaff(int taskId, int staffId)
        {
            var task = await _context.MaintenanceTasks
                .Include(t => t.Room)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedTo == staffId && t.Status != "Completed");

            if (task == null)
            {
                return null;
            }

            task.Status = "Completed";
            task.CompletedAt = DateTime.Now;

            if (task.Room != null)
            {
                task.Room.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<MaintenanceTask?> CompleteTaskByAdmin(int taskId)
        {
            var task = await _context.MaintenanceTasks
                .Include(t => t.Room)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return null;
            }

            task.Status = "Completed";
            task.CompletedAt = DateTime.Now;

            if (task.Room != null && task.Room.Status == "Maintenance")
            {
                task.Room.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<List<Room>> GetRoomsNotInMaintenance()
        {
            return await _context.Rooms
                .Where(r => r.Status != "Maintenance")
                .ToListAsync();
        }

        public async Task<List<User>> GetAssignableStaffUsers()
        {
            return await _context.Users
                .Where(u => u.Role == "Staff" || u.Role == "Admin")
                .ToListAsync();
        }

        public async Task<bool> AssignMaintenanceTask(MaintenanceTask task)
        {
            task.CreatedAt = DateTime.Now;
            task.Status = "In Progress";

            _context.MaintenanceTasks.Add(task);

            var room = await _context.Rooms.FindAsync(task.RoomId);
            if (room != null)
            {
                room.Status = "Maintenance";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateMaintenanceTask(int roomId, int staffUserId, string description, string priority, int creatorId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = new MaintenanceTask
                {
                    RoomId = roomId,
                    AssignedTo = staffUserId,
                    Description = description,
                    Priority = priority,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    Deadline = DateTime.Now.AddDays(1)
                };
                _context.MaintenanceTasks.Add(task);

                var room = await _context.Rooms.FindAsync(roomId);
                if (room != null) room.Status = "Maintenance";

                var notification = new Notification
                {
                    RecipientId = staffUserId,
                    SenderId = creatorId,
                    SenderName = "Hệ thống quản lý",
                    SenderType = "System",
                    RecipientType = "Staff",
                    Message = $"Bạn có nhiệm vụ bảo trì mới tại phòng {room?.RoomNumber}: {description}",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
