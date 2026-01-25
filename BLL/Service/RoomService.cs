using AutoMapper;
using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using DTOs.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _repository;
        private readonly IMapper _mapper;

        public RoomService(IGenericRepository<Room> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var room = await _repository.GetByIdAsync(id);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task AddRoomAsync(RoomDto roomDto)
        {
            var room = _mapper.Map<Room>(roomDto);
            await _repository.AddAsync(room);
        }

        public async Task UpdateRoomAsync(RoomDto roomDto)
        {
            var room = await _repository.GetByIdAsync(roomDto.Id);
            if (room != null)
            {
                _mapper.Map(roomDto, room);
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
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
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
    }
}
