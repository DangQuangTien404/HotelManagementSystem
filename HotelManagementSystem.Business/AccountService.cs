using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class AccountService
    {
        private readonly HotelManagementDbContext _context;
        public AccountService(HotelManagementDbContext context) => _context = context;

        public async Task<bool> RegisterStaff(User newUser, string position, string shift)
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
                    HireDate = DateTime.Now
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

        public async Task<bool> RegisterConsumer(User newUser, Customer newCustomer)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Check if username exists
                if (await _context.Users.AnyAsync(u => u.Username == newUser.Username))
                    return false;

                // 2. Save User (Role="Consumer")
                newUser.CreatedAt = DateTime.Now;
                newUser.Role = "Consumer"; // Enforce Consumer role
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 3. Save Customer linked to User
                newCustomer.UserId = newUser.Id;
                newCustomer.CreatedAt = DateTime.Now;

                // Handle empty strings for required fields to avoid DB errors
                if (string.IsNullOrWhiteSpace(newCustomer.Address)) newCustomer.Address = "N/A";
                if (string.IsNullOrWhiteSpace(newCustomer.IdentityNumber)) newCustomer.IdentityNumber = "N/A";
                // Email is usually required for login/contact, ensure it's set
                if (string.IsNullOrWhiteSpace(newCustomer.Email)) newCustomer.Email = newUser.Email;

                _context.Customers.Add(newCustomer);

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
