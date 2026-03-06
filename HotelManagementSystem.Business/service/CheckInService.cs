using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

using HotelManagementSystem.Business.interfaces;

namespace HotelManagementSystem.Business.service
{
    public class CheckInService : ICheckInService
    {
        private readonly HotelManagementDbContext _context;
        private readonly IRoomUpdateBroadcaster _broadcaster;

        public CheckInService(HotelManagementDbContext context, IRoomUpdateBroadcaster broadcaster)
        {
            _context = context;
            _broadcaster = broadcaster;
        }

        // Thêm tham số staffId vào đây
        public async Task<bool> ExecuteCheckIn(int reservationId, int staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var res = await _context.Reservations
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(r => r.Id == reservationId);
                if (res == null || res.Status != "Confirmed") return false;

                var checkInEntry = new CheckInOut
                {
                    ReservationId = reservationId,
                    CheckInTime = DateTime.Now,
                    CheckInBy = staffId,
                    TotalAmount = 0
                };
                _context.CheckInOuts.Add(checkInEntry);

                res.Status = "CheckedIn";
                if (res.Room != null)
                    res.Room.Status = "Occupied";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (res.Room != null)
                    await _broadcaster.BroadcastRoomStatusAsync(res.Room.Id, res.Room.RoomNumber, "Occupied");

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}