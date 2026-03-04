using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class CheckOutService
    {
        private readonly HotelManagementDbContext _context;
        private readonly IRoomUpdateBroadcaster _broadcaster;

        public CheckOutService(HotelManagementDbContext context, IRoomUpdateBroadcaster broadcaster)
        {
            _context = context;
            _broadcaster = broadcaster;
        }

        // Thêm tham số staffId vào đây
        public async Task<bool> ExecuteCheckOut(int reservationId, int staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var checkInfo = await _context.CheckInOuts
                    .Include(c => c.Reservation)
                    .ThenInclude(r => r.Room)
                    .FirstOrDefaultAsync(c => c.ReservationId == reservationId && c.CheckOutTime == null);

                if (checkInfo == null)
                {
                    // Fallback: reservation was checked in without a CheckInOut record
                    var res = await _context.Reservations
                        .Include(r => r.Room)
                        .FirstOrDefaultAsync(r => r.Id == reservationId && r.Status == "CheckedIn");

                    if (res == null || res.Room == null) return false;

                    checkInfo = new CheckInOut
                    {
                        ReservationId = reservationId,
                        CheckInTime = res.CheckInDate,
                        TotalAmount = 0
                    };
                    checkInfo.Reservation = res;
                    _context.CheckInOuts.Add(checkInfo);
                }

                checkInfo.CheckOutTime = DateTime.Now;
                checkInfo.CheckOutBy = staffId;

                var stayDuration = (checkInfo.CheckOutTime.Value - checkInfo.CheckInTime!.Value).Days;
                if (stayDuration <= 0) stayDuration = 1;

                var roomAmount = stayDuration * checkInfo.Reservation.Room.Price;
                var serviceAmount = await _context.ReservationServices
                    .Where(s => s.ReservationId == reservationId)
                    .SumAsync(s => s.Quantity * s.UnitPrice);

                var totalAmount = roomAmount + serviceAmount;
                checkInfo.TotalAmount = totalAmount;

                var depositPaid = await _context.Payments
                    .Where(p => p.ReservationId == reservationId && p.Status == "Completed")
                    .SumAsync(p => p.Amount);

                var remainingBalance = Math.Max(0, totalAmount - depositPaid);
                if (remainingBalance > 0)
                {
                    _context.Payments.Add(new Payment
                    {
                        ReservationId = reservationId,
                        PaymentMethod = "Cash",
                        OrderId = $"CHECKOUT_{reservationId}_{DateTime.Now:yyyyMMddHHmmss}",
                        Amount = remainingBalance,
                        Status = "Completed",
                        CreatedAt = DateTime.Now,
                        CompletedAt = DateTime.Now
                    });
                }

                checkInfo.Reservation.Status = "Completed";
                checkInfo.Reservation.Room.Status = "Available";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _broadcaster.BroadcastRoomStatusAsync(
                    checkInfo.Reservation.Room.Id,
                    checkInfo.Reservation.Room.RoomNumber,
                    "Available");

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