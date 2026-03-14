using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IChatbotService
    {
        Task<string> GetChatResponseAsync(string userMessage);
    }
}
