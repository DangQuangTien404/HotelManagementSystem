using HotelManagementSystem.Business.interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.service
{
    public class ChatbotService : IChatbotService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(Kernel kernel, ILogger<ChatbotService> logger)
        {
            _kernel = kernel;
            _logger = logger;
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

        public async IAsyncEnumerable<string> GetStreamingChatResponseAsync(string userMessage)
        {
            // Sử dụng IChatCompletionService trực tiếp để đảm bảo tính tương thích và hỗ trợ streaming tốt hơn
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Bạn là trợ lý ảo của Luxury Hotel. Hãy trả lời ngắn gọn, lịch sự.");
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
