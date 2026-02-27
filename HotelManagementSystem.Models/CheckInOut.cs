using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class CheckInOut
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public int? CheckInBy { get; set; }

    public int? CheckOutBy { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual User? CheckInByNavigation { get; set; }

    public virtual User? CheckOutByNavigation { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;
}
