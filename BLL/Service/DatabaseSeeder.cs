using BLL.Interfaces;
using DAL;
using DTOs.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly HotelDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DatabaseSeeder(HotelDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            // Look for any users.
            if (_context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = _passwordHasher.HashPassword("admin123"),
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

            _context.Users.Add(adminUser);
            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();
        }
    }
}
