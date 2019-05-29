using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core
{
    public class LoginManager
    {
        internal readonly Uri BaseAddress;
        internal readonly HttpClient HttpClient;
        internal string ConnectionToken;

        public LoginManager(string baseAddress)
        {
            BaseAddress = new Uri(baseAddress);
            HttpClient = new HttpClient
            {
                BaseAddress = BaseAddress
            };
        }

        public async Task AuthenticateAsync(string user, string pass, bool registerOnFailedLogin = true)
        {
            Console.WriteLine("Logging in...");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseAddress, "account/login"));
            var encoding = Encoding.GetEncoding("iso-8859-1");
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoding.GetBytes($"{user}:{pass}")));
            var response = await HttpClient.SendAsync(request);

            if (registerOnFailedLogin && !response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to login. Attempting registration with same credentials...");
                request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseAddress, "account/register"));
                response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Account registered.");
            }

            if (response?.IsSuccessStatusCode == true)
            {
                ConnectionToken = await response.Content.ReadAsStringAsync();
            }

            if (string.IsNullOrWhiteSpace(ConnectionToken)) throw new InvalidOperationException("Token missing from authentication response.");

            Console.WriteLine("Logged in.");
        }
    }
}