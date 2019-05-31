using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Core
{
    public class HubManager
    {
        private readonly Uri _hubUri;
        private readonly LoginManager _loginManager;
        private HubConnection _hubConnection;

        public HubManager(string hubRoot, LoginManager loginManager)
        {
            _loginManager = loginManager;
            _hubUri = new Uri(_loginManager.BaseAddress, hubRoot);
        }

        public async Task ConnectAsync(bool addDefaultListeners = true)
        {
            Console.WriteLine("Connecting to server...");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUri,
                    options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_loginManager.ConnectionToken);
                        options.Transports = HttpTransportType.WebSockets;
                    })
                .Build();

            if (addDefaultListeners)
            {
                _hubConnection.On(HubMessages.HubMethod.Connected.GetHubMethodName(), (string message) => Console.WriteLine(message));
                _hubConnection.On(HubMessages.HubMethod.Disconnected.GetHubMethodName(), (string message) => Console.WriteLine(message));
                _hubConnection.On(HubMessages.HubMethod.ReceiveMessage.GetHubMethodName(), (string message) =>
                {
                    if (!message.Substring(0, message.IndexOf(':')).Equals(_loginManager.User, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(message);
                    }
                });
            }

            await _hubConnection.StartAsync();
        }

        public async Task SendAsync(HubMessages.HubMethod hubMethod, string input) => await _hubConnection.InvokeAsync(hubMethod.GetHubMethodName(), input);
    }
}