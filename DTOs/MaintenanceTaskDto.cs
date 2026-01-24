using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class MaintenanceTaskDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;

        [Display(Name = "Assigned Staff")]
        public int? AssignedTo { get; set; }
        public string AssignedStaffName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Deadline is required")]
        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Issue description is required")]
        [Display(Name = "Issue Description")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
    }
}

