using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using DTOs.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class StaffDashboardService : IStaffDashboardService
    {
        private readonly IGenericRepository<Room> _roomRepository;

        public StaffDashboardService(IGenericRepository<Room> roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<StaffDashboardDto> GetDashboardDataAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();

            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Capacity = r.Capacity,
                Price = r.Price,
                Status = r.Status
            }).ToList();

            return new StaffDashboardDto
            {
                // Các biến đếm (Count)
                OccupiedRoomsCount = roomDtos.Count(r => r.Status == "Occupied"),
                ReservedRoomsCount = roomDtos.Count(r => r.Status == "Reserved"),
                AvailableRoomsCount = roomDtos.Count(r => r.Status == "Available"),
                MaintenanceRoomsCount = roomDtos.Count(r => r.Status == "Maintenance"),
                CleaningRoomsCount = roomDtos.Count(r => r.Status == "Cleaning"),

                // Các danh sách chi tiết (List) -> QUAN TRỌNG
                OccupiedRooms = roomDtos.Where(r => r.Status == "Occupied"),
                ReservedRooms = roomDtos.Where(r => r.Status == "Reserved"),
                CleaningRooms = roomDtos.Where(r => r.Status == "Cleaning"),
                MaintenanceRooms = roomDtos.Where(r => r.Status == "Maintenance") // Dòng này sửa lỗi logic hiển thị
            };
        }
    }
}