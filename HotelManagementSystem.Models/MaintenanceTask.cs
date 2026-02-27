using System;

namespace HotelManagementSystem.Models
{
    public partial class MaintenanceTask
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int? AssignedTo { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Bổ sung các trường này để sửa lỗi "does not contain a definition"
        public string? Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public virtual Room? Room { get; set; }
        public virtual User? Staff { get; set; }
    }
}