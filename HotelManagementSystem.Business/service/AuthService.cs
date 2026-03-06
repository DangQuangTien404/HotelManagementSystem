using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

using HotelManagementSystem.Business.interfaces;

namespace HotelManagementSystem.Business.service
{
    public class AuthService : IAuthService
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