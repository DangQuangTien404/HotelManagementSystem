using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Data.Models;

public partial class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation cho Bảo trì
    public virtual ICollection<MaintenanceTask> MaintenanceTaskAssignedToNavigations { get; set; } = new List<MaintenanceTask>();
    public virtual ICollection<MaintenanceTask> MaintenanceTaskApprovedByNavigations { get; set; } = new List<MaintenanceTask>();

    // Các Navigation khác
    public virtual ICollection<CheckInOut> CheckInOutCheckInByNavigations { get; set; } = new List<CheckInOut>();
    public virtual ICollection<CheckInOut> CheckInOutCheckOutByNavigations { get; set; } = new List<CheckInOut>();
    public virtual ICollection<Notification> NotificationRecipients { get; set; } = new List<Notification>();
    public virtual ICollection<Notification> NotificationSenders { get; set; } = new List<Notification>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<RoomCleaning> RoomCleanings { get; set; } = new List<RoomCleaning>();
    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    // Navigation property for Customer (Consumer role)
    // Note: A user might not have a Customer profile if they are purely Staff/Admin, but Consumers will have one.
    // However, since UserId is on Customer table, this is the inverse navigation.
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
