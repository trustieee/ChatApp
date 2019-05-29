using System;
using System.Threading.Tasks;
using ChatApp.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync(HubMessages.HubMethod.Connected.GetHubMethodName(), HubMessages.Notifications.UserJoined(Context.User.GetNameIdentifierClaim()));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync(HubMessages.HubMethod.Disconnected.GetHubMethodName(), HubMessages.Notifications.UserLeft(Context.User.GetNameIdentifierClaim()));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message) => await Clients.All.SendAsync(HubMessages.HubMethod.ReceiveMessage.GetHubMethodName(),
            HubMessages.Notifications.MessageClients(Context.User.GetNameIdentifierClaim(), message));
    }
}