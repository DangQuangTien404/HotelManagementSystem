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
        // 1. Thêm Repository của Room để cập nhật trạng thái phòng
        private readonly IGenericRepository<Room> _roomRepository;

        public async Task CreatePendingCleaningAsync(int roomId)
        {
            // 1. Create the Pending Task
            var cleaning = new RoomCleaning
            {
                RoomId = roomId,
                CleanedBy = null, // No one assigned yet
                CleaningDate = DateTime.Now,
                Status = "Pending"
            };
            await _repository.AddAsync(cleaning);

            // 2. Automatically mark Room as "Cleaning" (Dirty)
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room != null)
            {
                room.Status = DTOs.Enums.RoomStatus.Cleaning; // or RoomStatus.Dirty if you have that
                await _roomRepository.UpdateAsync(room);
            }
        }

        // Cập nhật Constructor để nhận thêm IGenericRepository<Room>
        public RoomCleaningService(IRoomCleaningRepository repository, IGenericRepository<Room> roomRepository)
        {
            _repository = repository;
            _roomRepository = roomRepository;
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
            // Tạo nhiệm vụ dọn dẹp
            var cleaning = new RoomCleaning
            {
                RoomId = roomId,
                CleanedBy = staffUserId,
                CleaningDate = DateTime.Now,
                Status = "Pending"
            };
            await _repository.AddAsync(cleaning);

            // 2. TỰ ĐỘNG CẬP NHẬT STATUS CỦA ROOM
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room != null)
            {
                // Đổi status phòng sang "Cleaning" (hoặc "Maintenance" tùy quy định của bạn)
                room.Status = DTOs.Enums.RoomStatus.Cleaning;
                await _roomRepository.UpdateAsync(room);
            }
        }

        public async Task UpdateTaskAsync(int cleaningId, string status, int? staffId)
        {
            var cleaning = await _repository.GetByIdAsync(cleaningId);
            if (cleaning != null)
            {
                // 1. Update Status
                cleaning.Status = status;

                // 2. Update Assigned Staff (NEW)
                if (staffId.HasValue)
                {
                    cleaning.CleanedBy = staffId.Value;
                }

                // 3. Handle Completion Logic (Same as before)
                if (status == "Completed")
                {
                    cleaning.CleaningDate = DateTime.Now;
                    var room = await _roomRepository.GetByIdAsync(cleaning.RoomId);
                    if (room != null)
                    {
                        room.Status = DTOs.Enums.RoomStatus.Available;
                        await _roomRepository.UpdateAsync(room);
                    }
                }

                await _repository.UpdateAsync(cleaning);
            }
        }

        public async Task DeleteCleaningAsync(int id)
        {
            // Tùy chọn: Nếu xóa lịch dọn dẹp thì có cần reset trạng thái phòng không? 
            // Nếu cần thì thêm logic ở đây tương tự như trên.
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