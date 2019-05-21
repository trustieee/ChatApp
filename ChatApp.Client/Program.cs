using ChatApp.Model;
using ChatApp.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Client
{
    class Program
    {
        static HubConnection HubConnection;
        static HttpClient Client;
        static Uri BaseAddress;

        static Program()
        {
            BaseAddress = new Uri("https://localhost:5001");
            Client = new HttpClient() { BaseAddress = BaseAddress };
        }

        static async Task Main(string[] args)
        {
            const string passwordPrompt = "Password: ";

            var user = new User
            {
                Password = string.Empty
            };

            while (true)
            {
                Console.Write("Username: ");
                string userName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    user.UserName = userName;
                    break;
                }

                Console.Clear();
            }

            var hasPass = false;
            Console.Write(passwordPrompt);
            StringBuilder passBuilder = new StringBuilder();
            while (!hasPass)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (Console.CursorLeft <= passwordPrompt.Length) break;
                        passBuilder.Remove(passBuilder.Length - 1, 1);
                        Console.Write("\b \b");
                        break;
                    case ConsoleKey.Enter:
                        if (passBuilder.Length <= 0)
                        {
                            Console.WriteLine("\nmissing password...");
                            Console.SetCursorPosition(passwordPrompt.Length, 1);
                            continue;
                        }
                        hasPass = true;
                        break;
                    default:
                        Console.Write('*');
                        passBuilder.Append(key.KeyChar);
                        break;
                }
            }

            var salt = "temp";
            var hash = Crypto.GetHashString(passBuilder.ToString(), salt);
            user.Password = hash;

            Console.Clear();
            Console.WriteLine("Logging in...");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseAddress, "account/login"));
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoding.GetBytes($"{user.UserName}:{user.Password}")));
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            Console.WriteLine("Logged in.");

            HubConnection = new HubConnectionBuilder()
                 .WithUrl(new Uri(BaseAddress, "chat"))
                 .Build();

            //HubConnection.Closed += async (error) =>
            //{
            //    await Task.Delay(new Random().Next(0, 5) * 1000);
            //    await HubConnection.StartAsync();
            //};

            HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine("Foo");
            });

            await HubConnection.StartAsync();

            Console.ReadKey();

            await HubConnection.InvokeAsync("SendMessage", "foo", "bar");

            Console.ReadKey();
        }
    }
}
