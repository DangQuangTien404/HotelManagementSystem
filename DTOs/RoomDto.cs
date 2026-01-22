namespace DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Available";
    }
}
