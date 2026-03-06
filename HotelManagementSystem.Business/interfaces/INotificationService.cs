using HotelManagementSystem.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface INotificationService
    {
        Task CreateAndSendNotificationAsync(Notification notification, bool toAdminGroup = false);
        Task<int> GetUnreadCount(int userId);
        Task<List<Notification>> GetLatestNotifications(int userId);
        Task MarkAsRead(int notificationId);
    }
}
