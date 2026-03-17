using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using System.ComponentModel;
using System.Text.Json;

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HotelManagementSystem.Business.service
{
    public class HotelDataPlugin
    {
        private readonly HotelManagementDbContext _context;
        private readonly ILogger<HotelDataPlugin> _logger;

        public HotelDataPlugin(HotelManagementDbContext context, ILogger<HotelDataPlugin> logger)
        {
            _context = context;
            _logger = logger;
        }

        [KernelFunction, Description("Lấy danh sách các loại phòng và giá tương ứng của khách sạn")]
        public async Task<string> GetRoomPricing()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[HotelDataPlugin] Bắt đầu lấy giá phòng...");
            try
            {
                var rooms = await _context.Rooms
                    .GroupBy(r => r.RoomType)
                    .Select(g => new { Type = g.Key, Price = g.Min(r => r.Price) }) // Dùng Min để lấy giá thấp nhất của loại phòng đó, tối ưu SQL hơn
                    .ToListAsync();

                _logger.LogInformation($"[HotelDataPlugin] Hoàn thành lấy giá phòng sau {sw.ElapsedMilliseconds}ms");
                return JsonSerializer.Serialize(new { Message = "Danh sách giá phòng", Data = rooms });
            }
            catch (Exception ex)
            {
                return $"Lỗi khi tra cứu giá phòng: {ex.Message}";
            }
        }

        [KernelFunction, Description("Kiểm tra xem hiện tại khách sạn còn những phòng nào trống (Available)")]
        public async Task<string> GetRoomAvailability()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[HotelDataPlugin] Bắt đầu kiểm tra phòng trống...");
            try
            {
                var availableRooms = await _context.Rooms
                    .Where(r => r.Status == "Available")
                    .Select(r => new { r.RoomNumber, r.RoomType, r.Price })
                    .ToListAsync();

                _logger.LogInformation($"[HotelDataPlugin] Hoàn thành kiểm tra phòng trống sau {sw.ElapsedMilliseconds}ms");
                return JsonSerializer.Serialize(new { Message = "Danh sách phòng trống", Data = availableRooms });
            }
            catch (Exception ex)
            {
                return $"Lỗi khi tra cứu phòng trống: {ex.Message}";
            }
        }

        [KernelFunction, Description("Lấy thông tin chi tiết về một phòng cụ thể dựa trên số phòng")]
        public async Task<string> GetRoomDetails(
            [Description("Số phòng cần tra cứu (ví dụ: '101', '54')")] string roomNumber)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation($"[HotelDataPlugin] Bắt đầu tra cứu chi tiết phòng {roomNumber}...");
            try
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

                _logger.LogInformation($"[HotelDataPlugin] Hoàn thành tra cứu phòng {roomNumber} sau {sw.ElapsedMilliseconds}ms");

                if (room == null)
                    return $"Không tìm thấy thông tin cho phòng số {roomNumber}.";

                return JsonSerializer.Serialize(new 
                { 
                    Message = $"Chi tiết phòng {roomNumber}", 
                    Data = new { room.RoomNumber, room.RoomType, room.Status, room.Price, room.Capacity } 
                });
            }
            catch (Exception ex)
            {
                return $"Lỗi khi tra cứu chi tiết phòng {roomNumber}: {ex.Message}";
            }
        }

        [KernelFunction, Description("Lấy tổng quan thông tin khách sạn bao gồm: danh sách các loại phòng, giá cả và trạng thái phòng trống hiện tại. Hãy ưu tiên dùng hàm này cho các câu hỏi chung về tình trạng phòng.")]
        public async Task<string> GetHotelGeneralInfo()
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[HotelDataPlugin] Bắt đầu lấy thông tin tổng quan khách sạn...");
            try
            {
                // 1. Lấy giá và loại phòng
                var pricing = await _context.Rooms
                    .GroupBy(r => r.RoomType)
                    .Select(g => new { Type = g.Key, Price = g.Min(r => r.Price) })
                    .ToListAsync();

                // 2. Lấy số lượng phòng trống theo từng loại
                var availability = await _context.Rooms
                    .Where(r => r.Status == "Available")
                    .GroupBy(r => r.RoomType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                _logger.LogInformation($"[HotelDataPlugin] Hoàn thành lấy thông tin tổng quan sau {sw.ElapsedMilliseconds}ms");
                
                return JsonSerializer.Serialize(new 
                { 
                    Message = "Thông tin tổng quan khách sạn", 
                    Pricing = pricing, 
                    AvailableRoomsSummary = availability 
                });
            }
            catch (Exception ex)
            {
                return $"Lỗi khi tra cứu tổng quan: {ex.Message}";
            }
        }

        [KernelFunction, Description("Lấy danh sách các dịch vụ đi kèm của khách sạn (Spa, Gym, Nhà hàng...)")]
        public async Task<string> GetHotelServices()
        {
            try
            {
                var services = await _context.HotelServices
                    .Where(s => s.IsActive) // CHỈ LẤY dịch vụ đang hoạt động
                    .Select(s => new { s.Name, s.Price })
                    .ToListAsync();

                return JsonSerializer.Serialize(new { Message = "Danh sách dịch vụ khách sạn", Data = services });
            }
            catch (Exception ex)
            {
                return $"Lỗi khi tra cứu dịch vụ: {ex.Message}";
            }
        }
    }
}
