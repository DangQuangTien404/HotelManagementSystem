using System;
using DTOs.Enums;

namespace DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal RoomPrice { get; set; }
        public int? ReservedBy { get; set; }
        public string ReservedByName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public decimal TotalPrice { get; set; }
        public int NumberOfNights { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indicates whether the reservation can be cancelled.
        /// A reservation can only be cancelled if it is confirmed and the check-in date is in the future.
        /// </summary>
        public bool CanCancel => Status == ReservationStatus.Confirmed && CheckInDate > DateTime.UtcNow;
    }
}
