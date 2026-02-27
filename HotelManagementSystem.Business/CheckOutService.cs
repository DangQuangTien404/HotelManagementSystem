using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class CheckOutService
    {
        private readonly HotelManagementDbContext _context;
        public CheckOutService(HotelManagementDbContext context) => _context = context;

        // Thêm tham số staffId vào đây
        public async Task<bool> ExecuteCheckOut(int reservationId, int staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm bản ghi CheckInOut đang mở (chưa có giờ trả phòng)
                var checkInfo = await _context.CheckInOuts
                    .Include(c => c.Reservation)
                    .ThenInclude(r => r.Room)
                    .FirstOrDefaultAsync(c => c.ReservationId == reservationId && c.CheckOutTime == null);

                if (checkInfo == null) return false;

                checkInfo.CheckOutTime = DateTime.Now;
                checkInfo.CheckOutBy = staffId; // Lưu ID người thực hiện check-out

                // Logic tính tiền: (Giờ trả - Giờ đến) tính theo ngày
                var stayDuration = (checkInfo.CheckOutTime.Value - checkInfo.CheckInTime.Value).Days;
                if (stayDuration <= 0) stayDuration = 1;

                var roomAmount = stayDuration * checkInfo.Reservation.Room.Price;
                var services = await _context.ReservationServices
                    .Where(s => s.ReservationId == reservationId)
                    .ToListAsync();
                var serviceAmount = services.Sum(s => s.Quantity * s.UnitPrice);

                checkInfo.TotalAmount = roomAmount + serviceAmount;

                // Cập nhật trạng thái
                checkInfo.Reservation.Status = "Completed";
                checkInfo.Reservation.Room.Status = "Available";

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

        public async Task<bool> ExecuteUserCheckOut(int reservationId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm bản ghi CheckInOut đang mở (chưa có giờ trả phòng)
                var checkInfo = await _context.CheckInOuts
                    .Include(c => c.Reservation)
                    .ThenInclude(r => r.Room)
                    .FirstOrDefaultAsync(c => c.ReservationId == reservationId && c.CheckOutTime == null);

                if (checkInfo == null) return false;

                checkInfo.CheckOutTime = DateTime.Now;
                // Nếu người dùng tự checkout, có thể để CheckOutBy là null hoặc lưu ID người dùng nếu schema cho phép
                // Trong User model, CheckOutBy liên kết với User, nên có thể lưu userId
                checkInfo.CheckOutBy = userId;

                // Logic tính tiền: (Giờ trả - Giờ đến) tính theo ngày
                var stayDuration = (checkInfo.CheckOutTime.Value - checkInfo.CheckInTime.Value).Days;
                if (stayDuration <= 0) stayDuration = 1;

                var roomAmount = stayDuration * checkInfo.Reservation.Room.Price;
                var services = await _context.ReservationServices
                    .Where(s => s.ReservationId == reservationId)
                    .ToListAsync();
                var serviceAmount = services.Sum(s => s.Quantity * s.UnitPrice);

                checkInfo.TotalAmount = roomAmount + serviceAmount;

                // Cập nhật trạng thái
                checkInfo.Reservation.Status = "Completed";
                checkInfo.Reservation.Room.Status = "Dirty"; // Yêu cầu user: khi user tự checkout, phòng chuyển sang "Dirty"

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