using Microsoft.AspNetCore.SignalR;
using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatbotService _chatbotService;
        private readonly HotelManagementDbContext _context;

        public ChatHub(IChatbotService chatbotService, HotelManagementDbContext context)
        {
            _chatbotService = chatbotService;
            _context = context;
        }

        public async Task SendMessage(string message)
        {
            // 1. Lấy UserId từ Claims (giả định dùng Cookie Auth với NameIdentifier)
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                // Nếu không tìm thấy UserId chính thống, thử tìm theo Name (Username) trong DB
                var username = Context.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null) userId = user.Id;
                    else return; // Không xác định được user
                }
                else return;
            }

            // 2. Lưu tin nhắn của User vào DB
            var userMsg = new ChatMessage
            {
                UserId = userId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.Now
            };
            _context.ChatMessages.Add(userMsg);
            await _context.SaveChangesAsync();

            // 3. Bắt đầu streaming phản hồi và tích lũy nội dung
            var fullResponse = new StringBuilder();
            await foreach (var chunk in _chatbotService.GetStreamingChatResponseAsync(message, userId))
            {
                fullResponse.Append(chunk);
                await Clients.Caller.SendAsync("ReceiveChunk", chunk);
            }
            
            // 4. Lưu tin nhắn của Assistant vào DB sau khi xong
            var aiMsg = new ChatMessage
            {
                UserId = userId,
                Role = "assistant",
                Content = fullResponse.ToString(),
                CreatedAt = DateTime.Now
            };
            _context.ChatMessages.Add(aiMsg);
            await _context.SaveChangesAsync();

            // 5. Thông báo kết thúc streaming
            await Clients.Caller.SendAsync("Finished");
        }
    }
}
