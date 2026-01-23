using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
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
    }
}
