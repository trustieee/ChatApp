using System.Security;

namespace ChatApp.Model
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
