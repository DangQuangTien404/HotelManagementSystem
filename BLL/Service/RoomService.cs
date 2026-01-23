using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using DTOs.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _repository;

        public RoomService(IGenericRepository<Room> repository)
        {
            _repository = repository;
        }
        // Thêm hàm này vào cuối class RoomService
        public async Task UpdateRoomStatusAsync(int roomId, string status)
        {
            var room = await _repository.GetByIdAsync(roomId);
            if (room != null)
            {
                room.Status = status;
                await _repository.UpdateAsync(room);
            }
        }
        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _repository.GetAllAsync();
            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Capacity = r.Capacity,
                Price = r.Price,
                Status = r.Status
            });
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var room = await _repository.GetByIdAsync(id);
            if (room == null) return null;

            return new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                Price = room.Price,
                Status = room.Status
            };
        }

        public async Task AddRoomAsync(RoomDto roomDto)
        {
            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                RoomType = roomDto.RoomType,
                Capacity = roomDto.Capacity,
                Price = roomDto.Price,
                Status = "Available"
            };
            await _repository.AddAsync(room);
        }

        public async Task UpdateRoomAsync(RoomDto roomDto)
        {
            var room = await _repository.GetByIdAsync(roomDto.Id);
            if (room != null)
            {
                room.RoomNumber = roomDto.RoomNumber;
                room.RoomType = roomDto.RoomType;
                room.Capacity = roomDto.Capacity;
                room.Price = roomDto.Price;
                await _repository.UpdateAsync(room);
            }
        }
        public async Task DeleteRoomAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(string? searchTerm, RoomType? roomType, decimal? maxPrice)
        {
            var query = IQueryable.Empty<Room>();
            var rooms = await _repository.GetAllAsync();
            
            var availableRooms = rooms.Where(r => r.Status == RoomStatus.Available);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                availableRooms = availableRooms.Where(r => 
                    r.RoomNumber.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                    r.RoomType.ToString().Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase));
            }

            if (roomType.HasValue)
            {
                availableRooms = availableRooms.Where(r => r.RoomType == roomType.Value);
            }

            if (maxPrice.HasValue)
            {
                availableRooms = availableRooms.Where(r => r.Price <= maxPrice.Value);
            }

            return availableRooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Capacity = r.Capacity,
                Price = r.Price,
                Status = r.Status
            }).OrderBy(r => r.Price);
        }
    }
}
