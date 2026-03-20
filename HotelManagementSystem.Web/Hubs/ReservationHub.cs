using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HotelManagementSystem.Web.Hubs
{
    public class ReservationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                if (Context.User.IsInRole("Admin") || Context.User.IsInRole("Receptionist"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "ReservationAdmins");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                if (Context.User.IsInRole("Admin") || Context.User.IsInRole("Receptionist"))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ReservationAdmins");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
