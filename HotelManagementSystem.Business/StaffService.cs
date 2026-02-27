using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class StaffService
    {
        private readonly HotelManagementDbContext _context;
        public StaffService(HotelManagementDbContext context) => _context = context;

        // Lấy thông tin chi tiết nhân viên dựa trên UserId
        public async Task<Staff?> GetStaffByUserId(int userId)
        {
            return await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Cập nhật ca làm việc (Shift)
        public async Task<bool> UpdateShift(int staffId, string newShift)
        {
            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null) return false;

            staff.Shift = newShift;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}