using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetAllNotificationsWithDetailsAsync();
        Task<IEnumerable<Notification>> GetNotificationsByRecipientTypeAsync(string recipientType);
        Task<IEnumerable<Notification>> GetNotificationsByRecipientIdAsync(int recipientId);
        Task<IEnumerable<Notification>> GetAnnouncementsAsync();
        Task<Notification?> GetNotificationWithDetailsByIdAsync(int id);
    }
}

