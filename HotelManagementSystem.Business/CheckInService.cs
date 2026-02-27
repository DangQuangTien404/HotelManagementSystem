using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class CheckInService
    {
        private readonly HotelManagementDbContext _context;
        public CheckInService(HotelManagementDbContext context) => _context = context;

        // Thêm tham số staffId vào đây
        public async Task<bool> ExecuteCheckIn(int reservationId, int staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var res = await _context.Reservations.FindAsync(reservationId);
                if (res == null || res.Status != "Confirmed") return false;

                var checkInEntry = new CheckInOut
                {
                    ReservationId = reservationId,
                    CheckInTime = DateTime.Now,
                    CheckInBy = staffId, // Lưu ID người thực hiện check-in
                    TotalAmount = 0
                };
                _context.CheckInOuts.Add(checkInEntry);

                res.Status = "CheckedIn";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
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