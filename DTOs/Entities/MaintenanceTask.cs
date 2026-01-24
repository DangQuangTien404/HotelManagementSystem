using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class MaintenanceTask
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; } = null!;

        public int? AssignedTo { get; set; }
        [ForeignKey("AssignedTo")]
        public virtual User? AssignedStaff { get; set; }

        public string Priority { get; set; } = "Medium"; // High, Medium, Low
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int? ApprovedBy { get; set; }
        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }
    }
}

