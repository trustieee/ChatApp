using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Model;
using ChatApp.Utilities;

namespace ChatApp.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var uri = new Uri("https://localhost:5001");
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

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "login"))
            {
                Content = new StringContent("foo", Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            Console.WriteLine("Logged in.");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
