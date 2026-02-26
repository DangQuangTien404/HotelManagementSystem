using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotelManagementSystem.Business
{
    public class DataCleanupService
    {
        private readonly HotelManagementDbContext _context;

        public DataCleanupService(HotelManagementDbContext context)
        {
            _context = context;
        }

        public async Task CleanDuplicateUsersAsync()
        {
            // 1. Find usernames that have duplicates
            var duplicateUsernames = await _context.Users
                .GroupBy(u => u.Username)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToListAsync();

            if (!duplicateUsernames.Any()) return;

            foreach (var username in duplicateUsernames)
            {
                // Fetch users with related important data to decide the winner
                var users = await _context.Users
                    .Where(u => u.Username == username)
                    .Include(u => u.Staff)
                    .Include(u => u.RoomCleanings)
                    .ToListAsync();

                // 2. Determine the "Winner" (User to keep)
                // Strategy:
                // - Priority 1: Has Staff record
                // - Priority 2: Has RoomCleaning tasks
                // - Priority 3: Highest ID (most recently created)

                var winner = users
                    .OrderByDescending(u => u.Staff.Any()) // True (1) > False (0)
                    .ThenByDescending(u => u.RoomCleanings.Any())
                    .ThenByDescending(u => u.Id)
                    .First();

                var losers = users.Where(u => u.Id != winner.Id).ToList();

                // 3. Merge Data from Losers to Winner
                foreach (var loser in losers)
                {
                    // Reassign RoomCleanings
                    // Use Query to get all tasks, even if not loaded in Includes (though we included them)
                    // Better to query DB directly to be sure we get everything
                    var cleaningTasks = await _context.RoomCleanings.Where(rc => rc.CleanedBy == loser.Id).ToListAsync();
                    foreach (var task in cleaningTasks)
                    {
                        task.CleanedBy = winner.Id;
                    }

                    // Reassign Staff records
                    var staffRecords = await _context.Staffs.Where(s => s.UserId == loser.Id).ToListAsync();
                    foreach (var staff in staffRecords)
                    {
                        staff.UserId = winner.Id;
                    }

                    // Reassign other potential related data
                    // CheckInOuts (CheckInBy)
                    var checkIns = await _context.CheckInOuts.Where(c => c.CheckInBy == loser.Id).ToListAsync();
                    foreach (var ci in checkIns) ci.CheckInBy = winner.Id;

                    // CheckInOuts (CheckOutBy)
                    var checkOuts = await _context.CheckInOuts.Where(c => c.CheckOutBy == loser.Id).ToListAsync();
                    foreach (var co in checkOuts) co.CheckOutBy = winner.Id;

                    // MaintenanceTasks (ApprovedBy)
                    var mtApproved = await _context.MaintenanceTasks.Where(m => m.ApprovedBy == loser.Id).ToListAsync();
                    foreach (var m in mtApproved) m.ApprovedBy = winner.Id;

                    // MaintenanceTasks (AssignedTo)
                    var mtAssigned = await _context.MaintenanceTasks.Where(m => m.AssignedTo == loser.Id).ToListAsync();
                    foreach (var m in mtAssigned) m.AssignedTo = winner.Id;

                    // Notifications (Recipient)
                    var notifRec = await _context.Notifications.Where(n => n.RecipientId == loser.Id).ToListAsync();
                    foreach (var n in notifRec) n.RecipientId = winner.Id;

                    // Notifications (Sender)
                    var notifSend = await _context.Notifications.Where(n => n.SenderId == loser.Id).ToListAsync();
                    foreach (var n in notifSend) n.SenderId = winner.Id;

                    // Reservations (ReservedBy)
                    var reservations = await _context.Reservations.Where(r => r.ReservedBy == loser.Id).ToListAsync();
                    foreach (var r in reservations) r.ReservedBy = winner.Id;

                    // Finally, remove the loser
                    _context.Users.Remove(loser);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
