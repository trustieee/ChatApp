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
            await Clients.All.SendAsync(HubMessages.Methods.Connected, HubMessages.Notifications.UserJoined(Context.User.Identity.Name));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync(HubMessages.Methods.Disconnected, HubMessages.Notifications.UserLeft(Context.User.Identity.Name));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message) => await Clients.All.SendAsync(HubMessages.Methods.ReceiveMessage, HubMessages.Notifications.MessageClients(Context.User.Identity.Name, message));
    }
}