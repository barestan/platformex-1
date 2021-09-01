using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex.Application;
using Platformex.Infrastructure;
using Platformex.Web;
using Platformex.Web.GraphQL;
using Platformex.Web.Swagger;
using Siam.Application;
using Siam.Application.Queries;
using Siam.Data.MemoContext;
using Siam.Data.QueryHandlers;
using Siam.MemoContext;
using Siam.MemoContext.Domain;

namespace Siam.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })

            .UseOrleans(builder =>
            {
                builder
                //Конфигурация кластера
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MemoDemo";
                })
                .Configure<EndpointOptions>(options 
                    => options.AdvertisedIPAddress = IPAddress.Loopback)
                .UseDashboard(options => { options.Port = 8081; }) 

                //Конфигурация шины событий
                .AddSimpleMessageStreamProvider("EventBusProvider")
                .AddMemoryGrainStorage("PubSubStore")

                //Конфигурация Reminders (для запуска регулярных задач)
                .UseInMemoryReminderService()

                //Конфигурация журналирования
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })

                //Конфигурация приложения
                .ConfigurePlatformex(p =>
                {
                    p.RegisterAggregate<MemoId, MemoAggregate, MemoState>()
                        .WithCommands();
                    p.RegisterApplicationParts<MemoListQuery>();
                    p.RegisterApplicationParts<MemoFromIdQueryHandler>();

                    //Конфигурация сервисов приложения (инфраструктуры)
                    p.ConfigureServices((services, configuration) =>
                    {
                        services.AddDbContext<MemoDbContext>(options 
                            => options.UseSqlServer(
                                configuration.GetConnectionString("SqlServer")));

                        services.AddScoped<IDbProvider<MemoModel>, MemoDbProvider>();
                    });

                    //Конфигурация WebAPI
                    p.ConfigureWebApi()
                        .WithOpenApi(options => options.Url = "swagger");

                    //Конфигурация GraphQL
                    p.ConfigureGraphQl(options => options.BasePath = "graphql")
                        .WithConsole(options => options.BasePath = "graphql-console");

                });

            });
        
    }
}
