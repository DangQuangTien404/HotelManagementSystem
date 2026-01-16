using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class CheckInOut
    {
        public int Id { get; set; }

        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; } = null!;

        // User who processed Check-In
        public int? CheckInBy { get; set; }
        [ForeignKey("CheckInBy")]
        public virtual User? CheckInStaff { get; set; }

        // User who processed Check-Out
        public int? CheckOutBy { get; set; }
        [ForeignKey("CheckOutBy")]
        public virtual User? CheckOutStaff { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal TotalAmount { get; set; }
    }
}