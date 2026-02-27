using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class NotificationService
    {
        private readonly HotelManagementDbContext _context;
        public NotificationService(HotelManagementDbContext context) => _context = context;

        // Lấy số lượng thông báo chưa đọc của một User
        public async Task<int> GetUnreadCount(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        // Lấy danh sách 5 thông báo mới nhất
        public async Task<List<Notification>> GetLatestNotifications(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
        }

        // Đánh dấu đã đọc
        public async Task MarkAsRead(int notificationId)
        {
            var noti = await _context.Notifications.FindAsync(notificationId);
            if (noti != null)
            {
                noti.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}