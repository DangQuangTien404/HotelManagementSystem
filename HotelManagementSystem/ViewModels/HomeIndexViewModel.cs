using DTOs;
using DTOs.Enums;
using System.Collections.Generic;

namespace HotelManagementSystem.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public string? SearchTerm { get; set; }
        public RoomType? SelectedRoomType { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || SelectedRoomType.HasValue || MaxPrice.HasValue;
    }
}
