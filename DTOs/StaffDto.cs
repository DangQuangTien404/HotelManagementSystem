using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class StaffDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required")]
        [Display(Name = "Position")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shift is required")]
        [Display(Name = "Shift")]
        public string Shift { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hire date is required")]
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty; // For create/update (required on create, optional on update)
    }
}

