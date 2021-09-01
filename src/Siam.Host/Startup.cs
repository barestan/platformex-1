using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Web.GraphQL;

namespace Siam.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Переработать GraphQL для поддержки асинхронных вызовов
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
            
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePlatformex();
        }
    }
}
