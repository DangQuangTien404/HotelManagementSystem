using System.Collections.Generic;

namespace DTOs
{
    public class StaffDashboardDto
    {
        public int OccupiedRoomsCount { get; set; }
        public int ReservedRoomsCount { get; set; }
        public int AvailableRoomsCount { get; set; }
        public int MaintenanceRoomsCount { get; set; }

        public IEnumerable<RoomDto> OccupiedRooms { get; set; } = new List<RoomDto>();
        public IEnumerable<RoomDto> ReservedRooms { get; set; } = new List<RoomDto>();
    }
}
