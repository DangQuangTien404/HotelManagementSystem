using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                if (Context.User.IsInRole("Admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                if (Context.User.IsInRole("Admin"))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}