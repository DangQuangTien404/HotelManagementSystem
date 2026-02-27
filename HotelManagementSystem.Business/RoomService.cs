using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class RoomService
    {
        private readonly HotelManagementDbContext _context;

        public RoomService(HotelManagementDbContext context) => _context = context;

        // Lấy toàn bộ danh sách phòng để hiển thị lên Dashboard
        public async Task<List<Room>> GetAllRooms()
        {
            return await _context.Rooms.ToListAsync();
        }
    }
}