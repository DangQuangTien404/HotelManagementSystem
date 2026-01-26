using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminNotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
        Task<IEnumerable<NotificationDto>> GetIncomingNotificationsAsync(); // Only from Customer and Staff
        Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientTypeAsync(string recipientType);
        Task<IEnumerable<NotificationDto>> GetAnnouncementsAsync();
        Task<NotificationDto?> GetNotificationByIdAsync(int id);
        Task SendNotificationToStaffAsync(NotificationDto notificationDto, int adminId);
        Task SendNotificationToCustomerAsync(NotificationDto notificationDto, int adminId);
        Task CreateAnnouncementAsync(NotificationDto notificationDto, int adminId);
        Task MarkAsReadAsync(int notificationId);
    }
}

