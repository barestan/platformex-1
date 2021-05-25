using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Demo.Application;
using Demo.Infrastructure;
using Demo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Platformex.Application;
using Platformex.Web;
using Platformex.Web.Swagger;

namespace Demo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            //???????????? ??????????????
            services.AddDbContext<DemoContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));
            services.AddSingleton<IMongoClient>(new MongoClient(Configuration.GetConnectionString("Mongo")));
            services.AddScoped(c =>
                c.GetService<IMongoClient>()?.StartSession());

            services.AddScoped<IDbProvider<ICarModel>, CarDbProvider>();
            services.AddScoped<IDbProvider<IDocumentModel>, DocumentDbProvider>();
            
            services.AddSingleton<IApiDescriptionGroupCollectionProvider,
                                    CommandsApiDescriptionGroupCollectionProvider>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseHttpsRedirection();
            app.UseSwagger();
            
            var options = app.ApplicationServices.GetRequiredService<EventFlySwaggerOptions>();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/" + options.Url.Trim('/') + "/v1/swagger.json", options.Name);

                //c.OAuthClientId("swaggerui");
                //c.OAuthAppName("Swagger UI");
            });

            app.UseMiddleware<PlatformexMiddleware>();
        }
    }
}
