using System;
using System.Net.Http;
using System.Threading.Tasks;
using ChatApp.Core;

namespace ChatApp.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var loginManager = new LoginManager("https://localhost:5001");
            var hubManager = new HubManager("chat", loginManager);

            while (true)
            {
                try
                {
                    var user = ClientInputManager.GetInput("Username: ");
                    var pass = ClientInputManager.GetInput("Password: ", true);

                    Console.Clear();
                    await loginManager.AuthenticateAsync(user, pass);

                    break;
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Failed to connect to server. It may not be running. Please try again.");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            await hubManager.ConnectAsync();

            Console.CancelKeyPress += async (s, e) =>
            {
                e.Cancel = true;
                await CloseAsync();
            };

            while (true)
            {
                var input = ClientInputManager.GetInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                foreach (var command in ClientInputManager.Process(input))
                {
                    switch (command)
                    {
                        case ClientInputManager.InputCommand.Exit:
                            await CloseAsync();
                            break;
                        default:
                            Console.WriteLine("Unrecognized client command.");
                            break;
                    }
                }

                await hubManager.SendAsync(HubMessages.HubMethod.SendMessage, input);
            }
        }

        private static async Task CloseAsync()
        {
            Console.WriteLine("Closing ChatApp...");
            Console.WriteLine("Thank you for using ChatApp!");
            await Task.Delay(TimeSpan.FromSeconds(2));
            Environment.Exit(0);
        }
    }
}