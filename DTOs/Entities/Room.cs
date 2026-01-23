using System.Collections.Generic;
using DTOs.Enums;

namespace DTOs.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; } = RoomType.Single;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        // Navigation
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<RoomCleaning> Cleanings { get; set; } = new List<RoomCleaning>();
    }
}