using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class CleaningService
    {
        private readonly HotelManagementDbContext _context;
        private readonly NotificationService _notificationService;

        public CleaningService(HotelManagementDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

            var user = await _context.Users.FindAsync(userId);
            string staffName = user?.FullName ?? "Nhân viên";
            string roomNumber = task.Room?.RoomNumber ?? "Không xác định";

            await _notificationService.CreateAndSendNotificationAsync(new Notification
            {
                Message = $"Nhân viên {staffName} đã hoàn thành dọn dẹp phòng {roomNumber}.",
                SenderName = "Hệ thống",
                SenderType = "System",
                RecipientType = "Admin",
                IsAnnouncement = true
            }, toAdminGroup: true);

            return true;
        }
    }
}
