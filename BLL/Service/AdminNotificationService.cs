using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public AdminNotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
        {
            var notifications = await _notificationRepository.GetAllNotificationsWithDetailsAsync();
            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetIncomingNotificationsAsync()
        {
            // Only get notifications from Customer and Staff (not from Admin)
            var notifications = await _notificationRepository.GetAllNotificationsWithDetailsAsync();
            return notifications
                .Where(n => n.SenderType == "Customer" || n.SenderType == "Staff")
                .Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientTypeAsync(string recipientType)
        {
            var notifications = await _notificationRepository.GetNotificationsByRecipientTypeAsync(recipientType);
            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDto>> GetAnnouncementsAsync()
        {
            var notifications = await _notificationRepository.GetAnnouncementsAsync();
            return notifications.Select(MapToDto);
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            var notification = await _notificationRepository.GetNotificationWithDetailsByIdAsync(id);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task SendNotificationToStaffAsync(NotificationDto notificationDto, int adminId)
        {
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                throw new Exception("Admin user not found.");
            }

            var notification = new Notification
            {
                SenderId = adminId,
                SenderName = admin.FullName,
                SenderType = "Admin",
                RecipientType = "Staff",
                RecipientId = null, // All staff
                Message = notificationDto.Message,
                IsAnnouncement = false,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task SendNotificationToCustomerAsync(NotificationDto notificationDto, int adminId)
        {
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                throw new Exception("Admin user not found.");
            }

            var notification = new Notification
            {
                SenderId = adminId,
                SenderName = admin.FullName,
                SenderType = "Admin",
                RecipientType = notificationDto.RecipientId.HasValue ? "Specific" : "Customer",
                RecipientId = notificationDto.RecipientId,
                Message = notificationDto.Message,
                IsAnnouncement = false,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task CreateAnnouncementAsync(NotificationDto notificationDto, int adminId)
        {
            var admin = await _userRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                throw new Exception("Admin user not found.");
            }

            var notification = new Notification
            {
                SenderId = adminId,
                SenderName = admin.FullName,
                SenderType = "Admin",
                RecipientType = "All",
                RecipientId = null, // All users
                Message = notificationDto.Message,
                IsAnnouncement = true,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new Exception("Notification not found.");
            }

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                SenderId = notification.SenderId,
                SenderName = notification.SenderName,
                SenderType = notification.SenderType,
                RecipientType = notification.RecipientType,
                RecipientId = notification.RecipientId,
                RecipientName = notification.Recipient?.FullName ?? string.Empty,
                Message = notification.Message,
                IsAnnouncement = notification.IsAnnouncement,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }
    }
}

