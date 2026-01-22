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
                OccupiedRoomsCount = roomDtos.Count(r => r.Status == "Occupied"),
                ReservedRoomsCount = roomDtos.Count(r => r.Status == "Reserved"),
                AvailableRoomsCount = roomDtos.Count(r => r.Status == "Available"),
                MaintenanceRoomsCount = roomDtos.Count(r => r.Status == "Maintenance"),
                OccupiedRooms = roomDtos.Where(r => r.Status == "Occupied"),
                ReservedRooms = roomDtos.Where(r => r.Status == "Reserved")
            };
        }
    }
}
