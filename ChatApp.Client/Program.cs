using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Core;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client
{
    internal class Program
    {
        private static HubConnection _hubConnection;
        private static readonly HttpClient _client;
        private static readonly Uri _baseAddress;
        private static readonly CookieContainer _container;
        private static readonly ManualResetEventSlim _resetEvent;

        static Program()
        {
            _baseAddress = new Uri("https://localhost:5001");
            HttpMessageHandler handler = new HttpClientHandler
            {
                CookieContainer = _container ??= new CookieContainer()
            };
            _client = new HttpClient(handler) {BaseAddress = _baseAddress};
            _resetEvent = new ManualResetEventSlim(false);
        }

        private static async Task Main()
        {
            const string PasswordPrompt = "Password: ";
            var password = string.Empty;

            while (true)
            {
                try
                {
                    string userName;
                    while (true)
                    {
                        Console.Write("Username: ");
                        var userNameInput = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(userNameInput))
                        {
                            userName = userNameInput;
                            break;
                        }

                        Console.Clear();
                    }

                    Console.Write(PasswordPrompt);
                    var passBuilder = new StringBuilder();
                    while (true)
                    {
                        var keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            if (Console.CursorLeft <= PasswordPrompt.Length)
                            {
                                break;
                            }

                            passBuilder.Remove(passBuilder.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                        else if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            if (passBuilder.Length <= 0)
                            {
                                Console.WriteLine("\nmissing password...");
                                Console.SetCursorPosition(PasswordPrompt.Length, 1);
                                continue;
                            }

                            password = passBuilder.ToString();
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

                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseAddress, "account/login"));
                    var encoding = Encoding.GetEncoding("iso-8859-1");
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoding.GetBytes($"{userName}:{password}")));
                    var response = await _client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed to login. Attempting registration with same credentials...");
                        request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseAddress, "account/register"));
                        response = await _client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine("Account registered.");
                    }

                    break;
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Failed to connect to server. It may not be running. Please try again.");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            Console.WriteLine("Logged in.");
            Console.WriteLine("Connecting to chat room...");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_baseAddress, "chat"), options => { options.Cookies = _container; })
                .Build();

            _hubConnection.On(HubMessages.Methods.Connected, (string message) => Console.WriteLine(message));
            _hubConnection.On(HubMessages.Methods.Disconnected, (string message) => Console.WriteLine(message));
            _hubConnection.On(HubMessages.Methods.ReceiveMessage, (string message) => Console.WriteLine(message));

            await _hubConnection.StartAsync();

            Console.WriteLine("Connected");
            DisplayMenu();

            Console.CancelKeyPress += async (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Closing ChatApp...");
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _client.Dispose();
                Console.WriteLine("Thank you for using ChatApp!");
                await Task.Delay(TimeSpan.FromSeconds(2));
                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    var input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        await _hubConnection.InvokeAsync(HubMessages.Methods.SendMessage, input);
                    }
                }
                catch (Exception ex) when (ex is NullReferenceException || ex is ObjectDisposedException)
                {
                    // ignore
                }
            }
        }

        private static void DisplayMenu()
        {
        }
    }
}