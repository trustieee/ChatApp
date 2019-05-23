using ChatApp.Server.DbContexts;
using ChatApp.Server.Hubs;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Server
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder => { builder.ConnectionString = _configuration.GetConnectionString("DefaultConnection"); }));
            services.AddIdentity<ApplicationUser, IdentityRole>(setup =>
            {
                setup.Password.RequireDigit = false;
                setup.Password.RequiredLength = 1;
                setup.Password.RequiredUniqueChars = 0;
                setup.Password.RequireLowercase = false;
                setup.Password.RequireNonAlphanumeric = false;
                setup.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication(config => { config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; }).AddCookie();

            services.AddAuthorization();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
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
}