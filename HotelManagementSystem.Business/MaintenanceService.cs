using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class MaintenanceService
    {
        private readonly HotelManagementDbContext _context;
        private readonly NotificationService _notificationService;

        public MaintenanceService(HotelManagementDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Lấy danh sách nhân viên kỹ thuật từ bảng Staff
        public async Task<List<Staff>> GetTechnicalStaff()
        {
            return await _context.Staffs
                .Include(s => s.User)
                .Where(s => s.Position == "Technician" || s.Position == "Maintenance")
                .ToListAsync();
        }

        // Tạo yêu cầu bảo trì và gửi thông báo
        public async Task<bool> CreateMaintenanceTask(int roomId, int staffUserId, string description, string priority, int creatorId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Task bảo trì
                var task = new MaintenanceTask
                {
                    RoomId = roomId,
                    AssignedTo = staffUserId,
                    Description = description,
                    Priority = priority,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    Deadline = DateTime.Now.AddDays(1) // Mặc định deadline 1 ngày
                };
                _context.MaintenanceTasks.Add(task);

                // 2. Cập nhật trạng thái phòng sang Maintenance
                var room = await _context.Rooms.FindAsync(roomId);
                if (room != null) room.Status = "Maintenance";

                // 3. Tạo thông báo (Notification) cho nhân viên được giao
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
                await _notificationService.CreateAndSendNotificationAsync(notification, toAdminGroup: false);

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