using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Server
{
    public class ApplicationUser : IdentityUser
    {
        public static readonly Expression<Func<ApplicationUser, UserDTO>> Projection = c => new UserDTO
        {
            Id = c.Id,
            UserName = c.UserName
        };
    }

    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
    }

    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder =>
            {
                builder.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            }));
            services.AddIdentity<ApplicationUser, IdentityRole>(setup =>
            {
                setup.Password.RequireDigit = false;
                setup.Password.RequiredLength = 1;
                setup.Password.RequiredUniqueChars = 0;
                setup.Password.RequireLowercase = false;
                setup.Password.RequireNonAlphanumeric = false;
                setup.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie();

            services.AddAuthorization();

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(options =>
            {
                options.MapHealthChecks("/health");
                options.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                options.MapHub<ChatHub>("/chat");
            });
        }
    }

    public class ChatHub : Hub
    {
        [Authorize]
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }

    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var (headerUserName, headerPassword) = GetAuthLoginInformation(HttpContext);

            // sign user in
            var signInResult = await _signInManager.PasswordSignInAsync(headerUserName, headerPassword, false, false);
            if (!signInResult.Succeeded)
            {
                return Unauthorized();
            }

            return new OkResult();
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            var (headerUserName, headerPassword) = GetAuthLoginInformation(HttpContext);

            var userResult = await _userManager.FindByNameAsync(headerUserName);
            if (userResult != null)
            {
                // if they somehow get here, they're already logged in and are confirmed to be the requested user for registration,
                //  so just return 200 and let them continue on
                if (HttpContext.User.Identity.Name == userResult.UserName && HttpContext.User.Identity.IsAuthenticated)
                    return new OkResult();

                // user already exists, but someone else (who isn't logged in as that user) is trying to
                //  create an account with the same name
                return new UnauthorizedResult();
            }

            var user = new ApplicationUser
            {
                UserName = headerUserName,
            };

            var createResult = await _userManager.CreateAsync(user, headerPassword);
            if (!createResult.Succeeded)
            {
                // keep a 401 here to remain consistent with the rest of the possible status codes
                //  sent back from this endpoint - that way attackers won't know the difference
                //  between an account existing already or not
                return new UnauthorizedResult();
            }

            // sign the user in after creation
            await _signInManager.SignInAsync(user, false);
            return new OkResult();
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return new OkResult();
        }

        [HttpGet, Authorize]
        public IEnumerable<UserDTO> Users()
        {
            return _userManager.Users.Select(ApplicationUser.Projection);
        }

        private static (string, string) GetAuthLoginInformation(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].First().Substring("Basic ".Length).Trim();
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            var split = encoding.GetString(Convert.FromBase64String(authHeader)).Split(':');
            return (split[0], split[1]);
        }

        private static (string, string, string) GetAuthRegistrationInformation(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].First().Substring("Basic ".Length).Trim();
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            var split = encoding.GetString(Convert.FromBase64String(authHeader)).Split(':');
            return (split[0], split[1], split[2]);
        }
    }
}
