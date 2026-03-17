using HotelManagementSystem.Business.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Pages
{
    public class TestAiModel : PageModel
    {
        private readonly IChatbotService _chatbotService;

        public TestAiModel(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [BindProperty]
        public string UserMessage { get; set; } = string.Empty;

        public string AiResponse { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(UserMessage))
            {
                AiResponse = await _chatbotService.GetChatResponseAsync(UserMessage);
            }
            return Page();
        }
    }
}
