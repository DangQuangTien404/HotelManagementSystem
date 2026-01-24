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
                room.Status = "Cleaning";
                await _roomRepository.UpdateAsync(room);
            }
        }

        public async Task UpdateStatusAsync(int cleaningId, string status)
        {
            var cleaning = await _repository.GetByIdAsync(cleaningId);
            if (cleaning != null)
            {
                cleaning.Status = status;

                // Cập nhật thời gian nếu hoàn thành
                if (status == "Completed")
                {
                    cleaning.CleaningDate = DateTime.Now;

                    // 3. TỰ ĐỘNG TRẢ VỀ TRẠNG THÁI "AVAILABLE" KHI DỌN XONG
                    // Tìm phòng tương ứng để cập nhật lại
                    var room = await _roomRepository.GetByIdAsync(cleaning.RoomId);
                    if (room != null)
                    {
                        room.Status = "Available";
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