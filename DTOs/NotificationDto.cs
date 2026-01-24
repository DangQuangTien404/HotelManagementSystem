using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public int? SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderType { get; set; } = string.Empty;

        public string RecipientType { get; set; } = string.Empty;
        public int? RecipientId { get; set; }
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Is Announcement")]
        public bool IsAnnouncement { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}

