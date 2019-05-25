using System;
using System.Threading.Tasks;
using ChatApp.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync(HubMessages.Methods.Connected, HubMessages.Notifications.UserJoined(Context.User.Identity.Name));
            await base.OnConnectedAsync();
        }

        [Authorize]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync(HubMessages.Methods.Disconnected, HubMessages.Notifications.UserLeft(Context.User.Identity.Name));
            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task SendMessage(string message) => await Clients.All.SendAsync(HubMessages.Methods.ReceiveMessage, HubMessages.Notifications.MessageClients(Context.User.Identity.Name, message));
    }
}