using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class MaintenanceTask
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public int? AssignedTo { get; set; }

    public string Priority { get; set; } = null!;

    public DateTime Deadline { get; set; }

    public string Status { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Room Room { get; set; } = null!;
}
