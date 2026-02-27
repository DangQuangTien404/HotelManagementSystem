using System.Collections.Generic;

namespace HotelManagementSystem.Models
{
    public class HotelService
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ReservationService> ReservationServices { get; set; } = new List<ReservationService>();
    }
}