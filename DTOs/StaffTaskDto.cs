using System.Collections.Generic;

namespace DTOs
{
    public class StaffTaskDto
    {
        public IEnumerable<RoomCleaningDto> MyCleaningTasks { get; set; } = new List<RoomCleaningDto>();
        public IEnumerable<RoomDto> MaintenanceRooms { get; set; } = new List<RoomDto>();
    }
}
