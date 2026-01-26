using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DTOs.Enums;

namespace DTOs.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int RoomId { get; set; }
        public virtual Room Room { get; set; } = null!;

        public int? ReservedBy { get; set; }
        [ForeignKey("ReservedBy")]
        public virtual User? ReservedByUser { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: 1-to-Many with CheckInOut
        public virtual ICollection<CheckInOut> CheckInOuts { get; set; } = new List<CheckInOut>();
    }
}