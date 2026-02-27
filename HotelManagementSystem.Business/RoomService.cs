using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
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

        // Thêm hoặc Cập nhật phòng
        public async Task SaveRoomAsync(Room room)
        {
            if (room.Id == 0)
            {
                room.Status = "Available"; // Mặc định phòng mới là Trống
                _context.Rooms.Add(room);
            }
            else
            {
                _context.Attach(room).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
    }
}
