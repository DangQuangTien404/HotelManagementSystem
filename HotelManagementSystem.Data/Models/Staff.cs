using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class Staff
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Position { get; set; } = null!;

    public string Shift { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public virtual User User { get; set; } = null!;
}
