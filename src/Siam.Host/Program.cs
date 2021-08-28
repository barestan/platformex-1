using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex.Infrastructure;
using Platformex.Web;
using Platformex.Web.GraphQL;
using Platformex.Web.Swagger;
using Siam.Application;
using Siam.Application.Queries;
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseOrleans(builder =>
                {
                    //Конфигурация кластера
                    builder.UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "HelloWorldApp";
                        })
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                        //Конфигурация шины событий
                        .AddSimpleMessageStreamProvider("EventBusProvider")
                        .AddMemoryGrainStorage("PubSubStore")

                        //Конфигурация журналирования
                        .ConfigureLogging(logging =>
                        {
                            logging.AddConsole();
                            logging.SetMinimumLevel(LogLevel.Information);
                        })

                        //Reminders в памяти
                        .UseInMemoryReminderService()

                        //Конфигурация приложения
                        .ConfigurePlatformex(p =>
                        {
                            p.RegisterAggregate<MemoId, MemoAggregate, MemoState>().WithCommands();
                            p.RegisterApplicationParts<MemoListQuery>();
                            p.RegisterApplicationParts<MemoFromIdQueryHandler>();

                            //Конфигурация WebAPI
                            p.ConfigureWebApi()
                                .WithSwagger(options =>
                                {
                                    options.Name = "Test";
                                    options.Url = "swagger";

                                });
                            p.ConfigureGraphQl(options => options.BasePath = "graphql")
                                .WithConsole(options => options.BasePath = "graphql-console");

                        })

                        //Orleans Dashboard
                        .UseDashboard(options => { options.Port = 8081;});
                });
        
    }
}
