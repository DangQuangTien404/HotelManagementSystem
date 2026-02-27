using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class Customer
{
    public int Id { get; set; }

    // Add UserId to link to the User table for authentication
    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string IdentityNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    // Navigation property
    public virtual User? User { get; set; }
}
