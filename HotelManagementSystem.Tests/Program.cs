using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Verification Test...");

            // 1. Setup InMemory DB
            var options = new DbContextOptionsBuilder<HotelManagementDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid()) // Unique DB name
                .Options;

            using (var context = new HotelManagementDbContext(options))
            {
                // 2. Seed Data
                var user1 = new User { Id = 1, Username = "testuser", FullName = "User One", Role = "Staff", PasswordHash="pass", Email="u1@test.com", CreatedAt=DateTime.Now };
                var user2 = new User { Id = 2, Username = "testuser", FullName = "User Two", Role = "Staff", PasswordHash="pass", Email="u2@test.com", CreatedAt=DateTime.Now.AddMinutes(1) };

                // Add Users
                context.Users.AddRange(user1, user2);

                // Add Room (needed for RoomCleaning FK sometimes, but InMemory might be lenient unless strict)
                // Let's add room to be safe
                var room1 = new Room { Id = 101, RoomNumber = "101", RoomType = "Standard", Status = "Dirty", Price = 100, BasePrice=100 };
                var room2 = new Room { Id = 102, RoomNumber = "102", RoomType = "Standard", Status = "Dirty", Price = 100, BasePrice=100 };
                context.Rooms.AddRange(room1, room2);

                // Add Tasks
                // Task for User 1 (should move to User 2)
                var task1 = new RoomCleaning { Id = 1, RoomId = 101, CleanedBy = 1, Status = "Pending", CleaningDate = DateTime.Now };
                // Task for User 2 (should stay)
                var task2 = new RoomCleaning { Id = 2, RoomId = 102, CleanedBy = 2, Status = "Pending", CleaningDate = DateTime.Now };
                context.RoomCleanings.AddRange(task1, task2);

                // Add Staff Record for User 2 (making it the winner)
                var staff2 = new Staff { Id = 1, UserId = 2, Position = "Cleaner", Shift = "Morning", HireDate = DateTime.Now };
                context.Staffs.Add(staff2);

                await context.SaveChangesAsync();

                Console.WriteLine("Data Seeded. Users: " + await context.Users.CountAsync());
                Console.WriteLine("Tasks for User 1: " + await context.RoomCleanings.CountAsync(t => t.CleanedBy == 1));
                Console.WriteLine("Tasks for User 2: " + await context.RoomCleanings.CountAsync(t => t.CleanedBy == 2));

                // 3. Run Cleanup
                var service = new DataCleanupService(context);
                await service.CleanDuplicateUsersAsync();

                // 4. Verify Results
                var usersRemaining = await context.Users.ToListAsync();
                var tasksUser1 = await context.RoomCleanings.Where(t => t.CleanedBy == 1).CountAsync();
                var tasksUser2 = await context.RoomCleanings.Where(t => t.CleanedBy == 2).CountAsync();
                var staffUser2 = await context.Staffs.Where(s => s.UserId == 2).CountAsync();

                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("Cleanup Complete.");
                Console.WriteLine($"Users Remaining: {usersRemaining.Count} (Expected: 1)");
                if (usersRemaining.Count == 1)
                {
                    Console.WriteLine($"Remaining User ID: {usersRemaining[0].Id} (Expected: 2)");
                    Console.WriteLine($"Remaining User Username: {usersRemaining[0].Username}");
                }

                Console.WriteLine($"Tasks for User 1 (Deleted): {tasksUser1} (Expected: 0)");
                Console.WriteLine($"Tasks for User 2 (Winner): {tasksUser2} (Expected: 2)");
                Console.WriteLine($"Staff Records for User 2: {staffUser2} (Expected: 1)");

                if (usersRemaining.Count == 1 && usersRemaining[0].Id == 2 && tasksUser1 == 0 && tasksUser2 == 2)
                {
                    Console.WriteLine("TEST PASSED!");
                }
                else
                {
                    Console.WriteLine("TEST FAILED!");
                    Environment.Exit(1);
                }
            }
        }
    }
}
