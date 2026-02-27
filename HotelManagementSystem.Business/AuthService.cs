using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class AuthService
    {
        private readonly HotelManagementDbContext _context;
        public AuthService(HotelManagementDbContext context) => _context = context;

        public async Task<User?> Login(string username, string password)
        {
            // Trong thực tế bạn nên dùng thư viện BCrypt để verify PasswordHash
            // Ở đây mình so sánh trực tiếp để bạn dễ hình dung luồng trước
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
        }
    }
}