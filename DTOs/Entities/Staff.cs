using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTOs.Entities
{
    public class Staff
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public string Position { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
    }
}