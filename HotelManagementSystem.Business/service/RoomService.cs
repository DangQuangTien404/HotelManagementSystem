using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

using HotelManagementSystem.Business.interfaces;

namespace HotelManagementSystem.Business.service
{
    public class RoomService : IRoomService
    {
        private readonly HotelManagementDbContext _context;
        private readonly IRoomUpdateBroadcaster _broadcaster;

        public RoomService(HotelManagementDbContext context, IRoomUpdateBroadcaster broadcaster)
        {
            _context = context;
            _broadcaster = broadcaster;
        }

        // Lấy toàn bộ danh sách phòng để hiển thị lên Dashboard
        public async Task<List<Room>> GetAllRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(string? search, string? type)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.RoomNumber.Contains(search) || r.RoomType.Contains(search));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(r => r.RoomType == type);

            return await query.OrderBy(r => r.RoomNumber).ToListAsync();
        }

        public async Task<List<string>> GetRoomTypesAsync()
        {
            return await _context.Rooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        // Thêm hoặc Cập nhật phòng
        public async Task SaveRoomAsync(Room room)
        {
            // Sync Price to BasePrice for backward compatibility
            room.Price = room.BasePrice;

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
            await _broadcaster.BroadcastRoomStatusAsync(room.Id, room.RoomNumber, room.Status ?? "Available");
        }
    }
}
