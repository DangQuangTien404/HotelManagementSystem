using System;

namespace DTOs
{
    public class RoomCleaningDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;

        public int? CleanedById { get; set; }
        public string CleanerName { get; set; } = string.Empty;

        public DateTime CleaningDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
