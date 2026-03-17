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
                return response.GetValue<string>() ?? "AI api đang bị lỗi!";
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

THÔNG TIN & QUY ĐỊNH CHÍNH THỨC CỦA LUXURY HOTEL (Sử dụng thông tin này làm gốc):
1. Vị trí: Tọa lạc tại trung tâm thành phố với view biển/thành phố tuyệt đẹp.
2. Giờ giấc: 
   - Nhận phòng (Check-in): Từ 14:00.
   - Trả phòng (Check-out): Trước 12:00 trưa.
3. Chính sách hủy phòng: Cần thông báo trước ít nhất 24 giờ so với giờ nhận phòng để được miễn phí hủy.
4. Quy định chung:
   - Tuyệt đối KHÔNG hút thuốc trong phòng và các khu vực công cộng có biển cấm.
   - KHÔNG cho phép mang theo thú cưng.
   - Vui lòng giữ yên tĩnh chung sau 22:00 để đảm bảo không gian nghỉ dưỡng cho các khách khác.
5. Tiện ích nổi bật: Hồ bơi vô cực, Phòng Gym 24/7, Nhà hàng ẩm thực Á-Âu, Dịch vụ Spa và đưa đón sân bay.

QUY TẮC PHẠM VI (SCOPE):
- BẠN CHỈ ĐƯỢC PHÉP trả lời về: Thông tin khách sạn, đặt phòng, giá phòng, dịch vụ, và các quy định đã nêu ở trên.
- Sử dụng Plugins (Tools) để tra cứu dữ liệu thực tế (Giá phòng, phòng trống, dịch vụ) từ Database TRƯỚC khi khẳng định thông tin.
- TUYỆT ĐỐI KHÔNG trả lời các câu hỏi về chính trị, tôn giáo, hoặc kiến thức không liên quan đến khách sạn.
- Nếu khách hỏi ngoài lề, trả lời: 'Dạ xin lỗi, em là trợ lý ảo của Luxury Hotel nên chỉ hỗ trợ các thông tin về khách sạn. Anh/chị cần em kiểm tra phòng giúp mình không ạ?'

QUY TẮC BẢO MẬT:
- Nếu Role là 'Guest'/'Anonymous': KHÔNG tiết lộ doanh thu, mật khẩu, dữ liệu cá nhân của khách khác hoặc thông tin kỹ thuật nội bộ.
- Phản hồi lịch sự: 'Dạ, thông tin này em không có quyền truy cập, anh/chị vui lòng liên hệ lễ tân để được hỗ trợ ạ.'

PHONG CÁCH PHẢN HỒI:
- Ngôn ngữ: Tiếng Việt, chuyên nghiệp, lịch sự, sử dụng 'Dạ', 'Anh/Chị'.
- Câu trả lời cần ngắn gọn, đi vào trọng tâm, tránh dài dòng.";

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
