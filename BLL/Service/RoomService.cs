using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using DTOs.Enums;
using Microsoft.EntityFrameworkCore;
using System;
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
        public async Task UpdateRoomStatusAsync(int roomId, RoomStatus status)
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
            return rooms.Select(MapToDto);
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var room = await _repository.GetByIdAsync(id);
            return room != null ? MapToDto(room) : null;
        }

        public async Task AddRoomAsync(RoomDto roomDto)
        {
            var room = MapToEntity(roomDto);
            await _repository.AddAsync(room);
        }

        public async Task UpdateRoomAsync(RoomDto roomDto)
        {
            var room = await _repository.GetByIdAsync(roomDto.Id);
            if (room != null)
            {
                room.RoomNumber = roomDto.RoomNumber;
                room.RoomType = roomDto.RoomType;
                room.Price = roomDto.Price;
                room.Status = roomDto.Status;
                room.Capacity = roomDto.Capacity;
                await _repository.UpdateAsync(room);
            }
        }
        public async Task DeleteRoomAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RoomDto>> SearchAvailableRoomsAsync(string? searchTerm, RoomType? roomType, decimal? maxPrice)
        {
            var query = _repository.GetQueryable();

            query = query.Where(r => r.Status == RoomStatus.Available);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.RoomNumber.Contains(searchTerm));
            }

            if (roomType.HasValue)
            {
                query = query.Where(r => r.RoomType == roomType.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.Price <= maxPrice.Value);
            }

            var rooms = await query.OrderBy(r => r.Price).ToListAsync();
            return rooms.Select(MapToDto);
        }

        public async Task BookRoom(int id)
        {
            var room = await _repository.GetByIdAsync(id);
            if (room != null && room.Status == RoomStatus.Available)
            {
                room.Status = RoomStatus.Occupied;
                await _repository.UpdateAsync(room);
            }
            var rooms = await _repository.GetQueryable()
                                         .Where(r => r.Status == RoomStatus.Available)
                                         .OrderBy(r => r.Price)
                                         .ToListAsync();
        }

        private static RoomDto MapToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Price = room.Price,
                Status = room.Status,
                Capacity = room.Capacity
            };
        }

        private static Room MapToEntity(RoomDto dto)
        {
            return new Room
            {
                Id = dto.Id,
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                Price = dto.Price,
                Status = dto.Status,
                Capacity = dto.Capacity
            };
        }
    }
}
