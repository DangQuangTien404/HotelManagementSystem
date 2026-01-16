using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class RoomCleaning
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; } = null!;

        public int? CleanedBy { get; set; }
        [ForeignKey("CleanedBy")]
        public virtual User? Cleaner { get; set; }

        public DateTime CleaningDate { get; set; }
        public string Status { get; set; } = "Completed"; // Pending, InProgress, Completed
    }
}