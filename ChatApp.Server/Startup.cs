using System;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Server.DbContexts;
using ChatApp.Server.Hubs;
using ChatApp.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //ValidIssuer = _configuration["Jwt:Issuer"],
                        //ValidAudience = _configuration["Jwt:Audience"],
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.Qv9aul8t7MKJowPkQSpHEbK17t0BA9gsi4Xt5flcpGA";

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/chat"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

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