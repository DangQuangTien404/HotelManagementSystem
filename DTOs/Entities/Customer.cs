using System;
using System.Collections.Generic;

namespace DTOs.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}