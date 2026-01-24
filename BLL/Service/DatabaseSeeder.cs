using BLL.Interfaces;
using DAL;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
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

            // Create MaintenanceTasks table if it doesn't exist
            try
            {
                var tableExists = await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaintenanceTasks]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[MaintenanceTasks] (
                            [Id] int NOT NULL IDENTITY(1,1),
                            [RoomId] int NOT NULL,
                            [AssignedTo] int NULL,
                            [Priority] nvarchar(max) NOT NULL,
                            [Deadline] datetime2 NOT NULL,
                            [Status] nvarchar(max) NOT NULL,
                            [Description] nvarchar(max) NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [CompletedAt] datetime2 NULL,
                            [ApprovedBy] int NULL,
                            CONSTRAINT [PK_MaintenanceTasks] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_MaintenanceTasks_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE,
                            CONSTRAINT [FK_MaintenanceTasks_Users_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_MaintenanceTasks_Users_AssignedTo] FOREIGN KEY ([AssignedTo]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                        );

                        CREATE INDEX [IX_MaintenanceTasks_ApprovedBy] ON [dbo].[MaintenanceTasks] ([ApprovedBy]);
                        CREATE INDEX [IX_MaintenanceTasks_AssignedTo] ON [dbo].[MaintenanceTasks] ([AssignedTo]);
                        CREATE INDEX [IX_MaintenanceTasks_RoomId] ON [dbo].[MaintenanceTasks] ([RoomId]);
                    END
                ");
            }
            catch
            {
                // Table might already exist, ignore error
            }

            // Create Notifications table if it doesn't exist
            try
            {
                var notificationsTableExists = await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Notifications] (
                            [Id] int NOT NULL IDENTITY(1,1),
                            [SenderId] int NULL,
                            [SenderName] nvarchar(max) NOT NULL,
                            [SenderType] nvarchar(max) NOT NULL,
                            [RecipientType] nvarchar(max) NOT NULL,
                            [RecipientId] int NULL,
                            [Message] nvarchar(max) NOT NULL,
                            [IsAnnouncement] bit NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [IsRead] bit NOT NULL,
                            CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_Notifications_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_Notifications_Users_RecipientId] FOREIGN KEY ([RecipientId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                        );

                        CREATE INDEX [IX_Notifications_SenderId] ON [dbo].[Notifications] ([SenderId]);
                        CREATE INDEX [IX_Notifications_RecipientId] ON [dbo].[Notifications] ([RecipientId]);
                    END
                ");
            }
            catch
            {
                // Table might already exist, ignore error
            }

            // Check if admin user already exists
            var adminExists = await _context.Users
                .AnyAsync(u => u.Username == "admin@" && u.Role == "Admin");

            if (!adminExists)
            {
                var adminUser = new User
                {
                    Username = "admin@",
                    PasswordHash = _passwordHasher.HashPassword("12345"),
                    Role = "Admin",
                    FullName = "System Administrator",
                    Email = "admin@hotel.com",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
