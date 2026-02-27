using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class RoomCleaning
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public int? CleanedBy { get; set; }

    public DateTime CleaningDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User? CleanedByNavigation { get; set; }

    public virtual Room Room { get; set; } = null!;
}
