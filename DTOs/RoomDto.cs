using DTOs.Enums;

namespace DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; } = RoomType.Single;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
    }
}
