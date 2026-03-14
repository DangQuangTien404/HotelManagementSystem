using HotelManagementSystem.Business.interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using System;
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
                // Sử dụng Semantic Kernel để gửi prompt tới OpenAI
                var response = await _kernel.InvokePromptAsync(userMessage);
                return response.GetValue<string>() ?? "Dạ, em không nhận được phản hồi từ AI.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi OpenAI API qua Semantic Kernel.");
                return $"Lỗi kết nối AI: {ex.Message}";
            }
        }
    }
}
