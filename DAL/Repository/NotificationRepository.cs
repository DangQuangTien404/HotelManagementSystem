using DAL;
using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsWithDetailsAsync()
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Recipient)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByRecipientTypeAsync(string recipientType)
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Recipient)
                .Where(n => n.RecipientType == recipientType)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByRecipientIdAsync(int recipientId)
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Recipient)
                .Where(n => n.RecipientId == recipientId || n.RecipientType == "All" || (n.RecipientType == "Staff" && n.RecipientId == null) || (n.RecipientType == "Customer" && n.RecipientId == null))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetAnnouncementsAsync()
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.IsAnnouncement == true)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetNotificationWithDetailsByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Recipient)
                .FirstOrDefaultAsync(n => n.Id == id);
        }
    }
}

