using HotelManagementSystem.Business.interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.service
{
    public class ChatbotService : IChatbotService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ChatbotService> _logger;
        private readonly HotelManagementDbContext _context;

        public ChatbotService(Kernel kernel, ILogger<ChatbotService> logger, HotelManagementDbContext context)
        {
            _kernel = kernel;
            _logger = logger;
            _context = context;
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            try
            {
                var response = await _kernel.InvokePromptAsync(userMessage);
                return response.GetValue<string>() ?? "Dạ, em không nhận được phản hồi từ AI.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi OpenAI API qua Semantic Kernel.");
                return $"Lỗi kết nối AI: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> GetStreamingChatResponseAsync(string userMessage, int userId, string? sessionId = null)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            
            chatHistory.AddSystemMessage("Bạn là trợ lý ảo của Luxury Hotel. Hãy trả lời ngắn gọn, lịch sự. Bạn có khả năng ghi nhớ các câu hỏi trước đó trong cùng một phiên hội thoại.");

            // 1. Tải lịch sử chat từ Database (10 tin nhắn gần nhất)
            var history = await _context.ChatMessages
                .Where(m => m.UserId == userId && m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            foreach (var msg in history)
            {
                if (msg.Role.ToLower() == "user")
                    chatHistory.AddUserMessage(msg.Content);
                else
                    chatHistory.AddAssistantMessage(msg.Content);
            }

            // 2. Thêm tin nhắn hiện tại của người dùng
            chatHistory.AddUserMessage(userMessage);

            var streamingResponse = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory);

            await foreach (var chunk in streamingResponse)
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    yield return chunk.Content;
                }
            }
        }
    }
}
