using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IChatbotService
    {
        Task<string> GetChatResponseAsync(string userMessage);
        IAsyncEnumerable<string> GetStreamingChatResponseAsync(string userMessage, int userId, string? sessionId = null, string? userRole = null);
    }
}
