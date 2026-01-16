using System.Collections.Generic;

namespace DTOs.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty; // Single, Double, Suite
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Available"; // Available, Occupied, Maintenance

        // Navigation
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<RoomCleaning> Cleanings { get; set; } = new List<RoomCleaning>();
    }
}