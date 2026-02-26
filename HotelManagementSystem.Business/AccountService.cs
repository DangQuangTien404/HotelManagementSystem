using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class AccountService
    {
        private readonly HotelManagementDbContext _context;
        public AccountService(HotelManagementDbContext context) => _context = context;

        public async Task<bool> RegisterStaff(User newUser, string position, string shift, DateTime? hireDate = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra username tồn tại chưa
                if (await _context.Users.AnyAsync(u => u.Username == newUser.Username))
                    return false;

                // 2. Lưu User
                newUser.CreatedAt = DateTime.Now;
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 3. Lưu thông tin Staff liên kết với User vừa tạo
                var staffEntry = new Staff
                {
                    UserId = newUser.Id,
                    Position = position,
                    Shift = shift,
                    HireDate = hireDate ?? DateTime.Now
                };
                _context.Staffs.Add(staffEntry);

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