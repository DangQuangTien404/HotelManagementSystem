using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int? SenderId { get; set; }

    public string SenderName { get; set; } = null!;

    public string SenderType { get; set; } = null!;

    public string RecipientType { get; set; } = null!;

    public int? RecipientId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsAnnouncement { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public virtual User? Recipient { get; set; }

    public virtual User? Sender { get; set; }
}
