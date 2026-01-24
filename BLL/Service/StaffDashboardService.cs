using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class StaffDashboardService : IStaffDashboardService
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IRoomCleaningService _cleaningService;

        public StaffDashboardService(IGenericRepository<Room> roomRepository, IRoomCleaningService cleaningService)
        {
            _roomRepository = roomRepository;
            _cleaningService = cleaningService;
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

        public async Task<StaffTaskDto> GetStaffTasksAsync(int staffUserId)
        {
            // 1. Get assigned cleaning tasks
            var cleaningTasks = await _cleaningService.GetCleaningsByStaffIdAsync(staffUserId);

            // 2. Get rooms in maintenance
            var allRooms = await _roomRepository.GetAllAsync();
            var maintenanceRooms = allRooms
                .Where(r => r.Status == "Maintenance")
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    Price = r.Price,
                    Status = r.Status
                });

            return new StaffTaskDto
            {
                MyCleaningTasks = cleaningTasks,
                MaintenanceRooms = maintenanceRooms
            };
        }
    }
}