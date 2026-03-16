using Microsoft.AspNetCore.SignalR;
using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HotelManagementSystem.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatbotService _chatbotService;
        private readonly HotelManagementDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatbotService chatbotService, HotelManagementDbContext context, ILogger<ChatHub> logger)
        {
            _chatbotService = chatbotService;
            _context = context;
            _logger = logger;
        }

        public async Task SendMessage(string message)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation($"[ChatHub] Nhận tin nhắn mới từ ConnectionId: {Context.ConnectionId}");
            
            // 1. Trích xuất thông tin định danh và Role
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                var username = Context.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null) 
                    {
                        userId = user.Id;
                        userRole = user.Role; // Lấy role từ DB nếu Claims chưa có
                    }
                    else return;
                }
                else return;
            }

            // 2. Lưu tin nhắn của User
            var userMsg = new ChatMessage
            {
                UserId = userId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.Now
            };
            var dbSw = Stopwatch.StartNew();
            _context.ChatMessages.Add(userMsg);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[ChatHub] Lưu tin nhắn User vào DB tốn {dbSw.ElapsedMilliseconds}ms");

            // 3. Streaming phản hồi với Role-based Context
            var aiSw = Stopwatch.StartNew();
            var fullResponse = new StringBuilder();
            try 
            {
                await foreach (var chunk in _chatbotService.GetStreamingChatResponseAsync(message, userId, null, userRole))
                {
                    fullResponse.Append(chunk);
                    await Clients.Caller.SendAsync("ReceiveChunk", chunk);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveChunk", $"[Lỗi hệ thống]: {ex.Message}");
            }
            
            // 4. Lưu tin nhắn của Assistant
            if (fullResponse.Length > 0)
            {
                var dbAiSw = Stopwatch.StartNew();
                var aiMsg = new ChatMessage
                {
                    UserId = userId,
                    Role = "assistant",
                    Content = fullResponse.ToString(),
                    CreatedAt = DateTime.Now
                };
                _context.ChatMessages.Add(aiMsg);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"[ChatHub] Lưu tin nhắn AI vào DB tốn {dbAiSw.ElapsedMilliseconds}ms");
            }

            _logger.LogInformation($"[ChatHub] Toàn bộ flow kết thúc trong {sw.ElapsedMilliseconds}ms");
            await Clients.Caller.SendAsync("Finished");
        }
    }
}
