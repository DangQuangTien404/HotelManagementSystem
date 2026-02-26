using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int RoomId { get; set; }

    public int? ReservedBy { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CheckInOut> CheckInOuts { get; set; } = new List<CheckInOut>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ReservedByNavigation { get; set; }

    public virtual Room Room { get; set; } = null!;
}
