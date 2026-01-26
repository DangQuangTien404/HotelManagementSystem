using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderType { get; set; } = string.Empty; // Customer, Staff, Admin

        public string RecipientType { get; set; } = string.Empty; // All, Staff, Customer, Specific
        public int? RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User? Recipient { get; set; }

        public string Message { get; set; } = string.Empty;
        public bool IsAnnouncement { get; set; } = false; // True for general announcements
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

