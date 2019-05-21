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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Server
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ChatApp"));
            services.AddIdentity<IdentityUser, IdentityRole>(setup =>
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

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthentication();
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
        public async Task SendMessage(string user, string message)
        {
            var foo = Context.User;
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }

    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            // get user creds
            IdentityUser user = new IdentityUser();
            var authHeader = HttpContext.Request.Headers["Authorization"].First().Substring("Basic ".Length).Trim();
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            var split = encoding.GetString(Convert.FromBase64String(authHeader)).Split(':');
            user.UserName = split[0];
            user.PasswordHash = split[1];

            // auth user
            //var foo = Crypto.Verify("bar", split[1], "temp");
            var identity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                },
                CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // sign user in
            var userResult = await _userManager.GetUserAsync(principal);
            if (userResult == null)
            {
                var createResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (!createResult.Succeeded)
                {
                    return new ForbidResult();
                }
            }

            // sign user in
            await _signInManager.SignInAsync(user, true);
            return Ok();
        }
    }
}
