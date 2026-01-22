using DTOs.Entities;
using System;
using System.Linq;

namespace DAL
{
    public static class DbInitializer
    {
        public static void Initialize(HotelDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = "admin123", // Plaintext for simplicity as requested
                Role = "Staff",
                FullName = "System Administrator",
                Email = "admin@hotel.com",
                CreatedAt = DateTime.UtcNow
            };

            // Add Staff entity
            var staff = new Staff
            {
                User = adminUser,
                Position = "Manager",
                Shift = "Day",
                HireDate = DateTime.Now
            };

            context.Users.Add(adminUser);
            context.Staffs.Add(staff);
            context.SaveChanges();
        }
    }
}
