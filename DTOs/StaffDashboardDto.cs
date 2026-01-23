using System.Collections.Generic;

namespace DTOs
{
    public class StaffDashboardDto
    {
        public int OccupiedRoomsCount { get; set; }
        public int ReservedRoomsCount { get; set; }
        public int AvailableRoomsCount { get; set; }
        public int MaintenanceRoomsCount { get; set; }
        public int CleaningRoomsCount { get; set; } // Thêm cái này

        public IEnumerable<RoomDto> OccupiedRooms { get; set; } = new List<RoomDto>();
        public IEnumerable<RoomDto> ReservedRooms { get; set; } = new List<RoomDto>();

        // BẮT BUỘC PHẢI CÓ 2 DÒNG NÀY ĐỂ HẾT LỖI:
        public IEnumerable<RoomDto> CleaningRooms { get; set; } = new List<RoomDto>();
        public IEnumerable<RoomDto> MaintenanceRooms { get; set; } = new List<RoomDto>();
    }
}