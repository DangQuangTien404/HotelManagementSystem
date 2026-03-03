using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class NotificationService
    {
        private readonly HotelManagementDbContext _context;
        private readonly IHubContext<Hub> _hubContext;

        public NotificationService(HotelManagementDbContext context, IHubContext<Hub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateAndSendNotificationAsync(Notification notification, bool toAdminGroup = false)
        {
            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Broadcast real-time notification
            if (toAdminGroup)
            {
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", notification.Message);
            }
            else if (notification.RecipientId.HasValue)
            {
                await _hubContext.Clients.User(notification.RecipientId.Value.ToString()).SendAsync("ReceiveNotification", notification.Message);
            }
        }

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