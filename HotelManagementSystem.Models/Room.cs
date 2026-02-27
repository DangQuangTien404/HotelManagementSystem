using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Room
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string RoomType { get; set; } = null!;

    public int Capacity { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public virtual ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = new List<MaintenanceTask>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<RoomCleaning> RoomCleanings { get; set; } = new List<RoomCleaning>();
}
