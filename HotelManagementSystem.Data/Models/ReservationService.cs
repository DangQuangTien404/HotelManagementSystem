using System;

namespace HotelManagementSystem.Data.Models;

public partial class ReservationService
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public int HotelServiceId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime AddedAt { get; set; }

    public int? AddedBy { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;

    public virtual HotelService HotelService { get; set; } = null!;
}
