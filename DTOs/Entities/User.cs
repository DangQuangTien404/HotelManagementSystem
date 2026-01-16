using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTOs.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property: 1-to-Many with Staff
        public virtual ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}