using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        [Authorize]
        public async Task SendMessage(string user, string message) => await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}