using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR(opt =>
            {
                opt.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouter((a) =>
            {

            });

            app.UseRouting(config =>
            {
                config.MapPost("/login", async (httpContext) =>
                {
                    using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
                    {
                        var foo = await reader.ReadToEndAsync();
                    }
                });
            });

            app.UseSignalR(conf =>
            {
                conf.MapHub<ChatHub>("/chat");
            });
        }
    }

    class ChatHub : Hub
    {

    }
}
