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

            // 1. Ensure Administrator exists
            var adminUser = _context.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = _passwordHasher.HashPassword("admin123"),
                    Role = "Administrator",
                    FullName = "System Administrator",
                    Email = "admin@hotel.com",
                    CreatedAt = DateTime.UtcNow
                };

                // Admin also gets a Staff record for simplicity in management
                var adminStaff = new Staff
                {
                    User = adminUser,
                    Position = "Manager",
                    Shift = "Day",
                    HireDate = DateTime.Now
                };

                _context.Users.Add(adminUser);
                _context.Staffs.Add(adminStaff);
            }
            else
            {
                // Update role if existing admin is not Administrator
                if (adminUser.Role != "Administrator")
                {
                    adminUser.Role = "Administrator";
                    _context.Users.Update(adminUser);
                }
            }

            // 2. Ensure a Regular Staff exists (for testing)
            if (!_context.Users.Any(u => u.Username == "staff"))
            {
                var staffUser = new User
                {
                    Username = "staff",
                    PasswordHash = _passwordHasher.HashPassword("staff123"),
                    Role = "Staff",
                    FullName = "Regular Staff",
                    Email = "staff@hotel.com",
                    CreatedAt = DateTime.UtcNow
                };

                var staffEntity = new Staff
                {
                    User = staffUser,
                    Position = "Housekeeping",
                    Shift = "Day",
                    HireDate = DateTime.Now
                };

                _context.Users.Add(staffUser);
                _context.Staffs.Add(staffEntity);
            }

            await _context.SaveChangesAsync();
        }
    }
}
