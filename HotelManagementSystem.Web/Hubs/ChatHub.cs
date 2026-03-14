using Microsoft.AspNetCore.SignalR;
using HotelManagementSystem.Business.interfaces;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatbotService _chatbotService;

        public ChatHub(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public async Task SendMessage(string message)
        {
            // Bắt đầu streaming phản hồi
            await foreach (var chunk in _chatbotService.GetStreamingChatResponseAsync(message))
            {
                await Clients.Caller.SendAsync("ReceiveChunk", chunk);
            }
            
            // Thông báo kết thúc streaming
            await Clients.Caller.SendAsync("Finished");
        }
    }
}
