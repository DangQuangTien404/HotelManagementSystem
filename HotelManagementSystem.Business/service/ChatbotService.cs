using HotelManagementSystem.Business.interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public async IAsyncEnumerable<string> GetStreamingChatResponseAsync(
            string userMessage, 
            int userId, 
            string? sessionId = null, 
            string? userRole = null)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation($"[ChatbotService] Bắt đầu xử lý AI cho User {userId}");
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            
            // 1. System Prompt kết hợp Bảo mật, Reasoning & Giới hạn phạm vi
            string systemPrompt = $@"Bạn là trợ lý ảo CHUYÊN BIỆT của Luxury Hotel. 
Quyền hạn của người dùng hiện tại (Role): {userRole ?? "Guest"}.

QUY TẮC PHẠM VI (SCOPE):
- BẠN CHỈ ĐƯỢC PHÉP trả lời các câu hỏi liên quan đến: Thông tin khách sạn, đặt phòng, giá phòng, dịch vụ khách sạn, quy định chung của Luxury Hotel.
- TUYỆT ĐỐI KHÔNG trả lời về các chủ đề ngoài lề
- Nếu khách hỏi ngoài lề hoặc không liên quan đến khách sạn hãy trả lời lịch sự: 'Dạ xin lỗi, em là trợ lý ảo chuyên trách của Luxury Hotel nên chỉ có thể hỗ trợ anh/chị các thông tin về khách sạn thôi ạ. Anh/chị cần em kiểm tra phòng giúp mình không ạ?'

QUY TẮC BẢO MẬT:
- Nếu Role là 'Guest' hoặc 'Anonymous': Bạn CHỈ được trả lời về giá phòng, phòng trống, dịch vụ chung. KHÔNG được tiết lộ doanh thu, thông tin bảo trì nội bộ hoặc dữ liệu khách hàng khác.
- Nếu không có quyền truy cập thông tin, hãy trả lời lịch sự: 'Xin lỗi, em không có quyền truy cập thông tin này, anh/chị vui lòng liên hệ lễ tân ạ.'

QUY TẮC PHẢN HỒI:
- Bạn có quyền sử dụng các công cụ (Plugins) để tra cứu dữ liệu thực tế từ Database. 
- Luôn ưu tiên tra cứu dữ liệu mới nhất trước khi khẳng định điều gì.
- Phản hồi bằng tiếng Việt, chuyên nghiệp, lịch sự.";

            chatHistory.AddSystemMessage(systemPrompt);

            // 2. Tải lịch sử chat
            var historySw = Stopwatch.StartNew();
            var history = await _context.ChatMessages
                .Where(m => m.UserId == userId && m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            _logger.LogInformation($"[ChatbotService] Tải lịch sử chat ({history.Count} tin) tốn {historySw.ElapsedMilliseconds}ms");

            foreach (var msg in history)
            {
                if (msg.Role.ToLower() == "user")
                    chatHistory.AddUserMessage(msg.Content);
                else
                    chatHistory.AddAssistantMessage(msg.Content);
            }

            // 3. Tin nhắn hiện tại
            chatHistory.AddUserMessage(userMessage);

            // 4. Kích hoạt Auto Function Calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var streamingResponse = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory: chatHistory,
                executionSettings: executionSettings,
                kernel: _kernel);

            var firstChunk = true;
            await foreach (var chunk in streamingResponse)
            {
                if (firstChunk) {
                    _logger.LogInformation($"[ChatbotService] AI bắt đầu phản hồi (Time to first chunk: {sw.ElapsedMilliseconds}ms)");
                    firstChunk = false;
                }

                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    yield return chunk.Content;
                }
            }
            _logger.LogInformation($"[ChatbotService] AI hoàn thành phản hồi. Tổng cộng: {sw.ElapsedMilliseconds}ms");
        }
    }
}
