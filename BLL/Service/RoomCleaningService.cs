using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class RoomCleaningService : IRoomCleaningService
    {
        private readonly IRoomCleaningRepository _repository;

        public RoomCleaningService(IRoomCleaningRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RoomCleaningDto>> GetAllCleaningsAsync()
        {
            var cleanings = await _repository.GetAllWithDetailsAsync();
            return cleanings.Select(MapToDto);
        }

        public async Task<IEnumerable<RoomCleaningDto>> GetPendingCleaningsAsync()
        {
            var cleanings = await _repository.GetPendingCleaningsAsync();
            return cleanings.Select(MapToDto);
        }

        public async Task AssignCleanerAsync(int roomId, int staffUserId)
        {
            var cleaning = new RoomCleaning
            {
                RoomId = roomId,
                CleanedBy = staffUserId,
                CleaningDate = DateTime.Now,
                Status = "Pending"
            };
            await _repository.AddAsync(cleaning);
        }

        public async Task UpdateStatusAsync(int cleaningId, string status)
        {
            var cleaning = await _repository.GetByIdAsync(cleaningId);
            if (cleaning != null)
            {
                cleaning.Status = status;
                if (status == "Completed")
                {
                    cleaning.CleaningDate = DateTime.Now; // Update completion time?
                }
                await _repository.UpdateAsync(cleaning);
            }
        }

        public async Task DeleteCleaningAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<RoomCleaningDto?> GetCleaningByIdAsync(int id)
        {
             var cleaning = await _repository.GetByIdWithDetailsAsync(id);

             if (cleaning == null) return null;
             return MapToDto(cleaning);
        }

        private RoomCleaningDto MapToDto(RoomCleaning rc)
        {
            return new RoomCleaningDto
            {
                Id = rc.Id,
                RoomId = rc.RoomId,
                RoomNumber = rc.Room?.RoomNumber ?? "Unknown",
                CleanedById = rc.CleanedBy,
                CleanerName = rc.Cleaner?.FullName ?? "Unassigned",
                CleaningDate = rc.CleaningDate,
                Status = rc.Status
            };
        }
    }
}
