using System;
using System.Linq;
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
            while (!hasPass)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (Console.CursorLeft <= passwordPrompt.Length) break;
                        user.Password = user.Password.Remove(user.Password.Length - 1);
                        Console.Write("\b \b");
                        break;
                    case ConsoleKey.Enter:
                        if (user.Password.Length <= 0)
                        {
                            Console.WriteLine("\nmissing password...");
                            Console.SetCursorPosition(passwordPrompt.Length, 1);
                            continue;
                        }
                        hasPass = true;
                        break;
                    default:
                        Console.Write('*');
                        user.Password += (key.KeyChar);
                        break;
                }
            }

            var salt = "";
            var hash = Crypto.GetHashString(user.Password, salt);

            Console.Clear();
            Console.WriteLine("Logging in...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine("Logged in.");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
