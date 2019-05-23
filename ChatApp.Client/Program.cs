using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client
{
    class Program
    {
        static HubConnection HubConnection;
        static readonly HttpClient Client;
        static readonly Uri BaseAddress;
        static readonly HttpMessageHandler Handler;
        static readonly CookieContainer Container;

        static Program()
        {
            BaseAddress = new Uri("https://localhost:5001");
            Handler = new HttpClientHandler
            {
                CookieContainer = Container ?? (Container = new CookieContainer())
            };
            Client = new HttpClient(Handler) { BaseAddress = BaseAddress };
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

            Console.Write(passwordPrompt);
            StringBuilder passBuilder = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft <= passwordPrompt.Length) break;
                    passBuilder.Remove(passBuilder.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (passBuilder.Length <= 0)
                    {
                        Console.WriteLine("\nmissing password...");
                        Console.SetCursorPosition(passwordPrompt.Length, 1);
                        continue;
                    }
                    user.Password = passBuilder.ToString();
                    break;
                }
                else
                {
                    Console.Write('*');
                    passBuilder.Append(keyInfo.KeyChar);
                }
            }

            Console.Clear();
            Console.WriteLine("Logging in...");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseAddress, "account/login"));
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoding.GetBytes($"{user.UserName}:{user.Password}")));
            var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to login. Attempting registration with same credentials...");
                request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseAddress, "account/register"));
                response = await Client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Account registered.");
            }

            Console.WriteLine("Logged in.");
            Console.WriteLine("Connecting to chat room...");

            HubConnection = new HubConnectionBuilder()
                 .WithUrl(new Uri(BaseAddress, "chat"), options =>
                 {
                     options.Cookies = Container;
                 })
                 .Build();

            HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine("Foo");
            });

            await HubConnection.StartAsync();

            Console.WriteLine("Connected. Press any key to send a message and receive a response.");
            Console.ReadKey();

            await HubConnection.InvokeAsync("SendMessage", "foo", "bar");
            Console.ReadKey();
        }
    }
}
