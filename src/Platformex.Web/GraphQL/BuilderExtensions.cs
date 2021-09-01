using System;
using GraphQL;
using GraphQL.Http;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;
using Platformex.Web.Swagger;

namespace Platformex.Web.GraphQL
{
    public sealed class EventFlyGraphQlOptions
    {
        public EventFlyGraphQlOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public sealed class EventFlyGraphQlConsoleOptions
    {
        public EventFlyGraphQlConsoleOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public static class BuilderExtensions
    {
        public static IApplicationBuilder UsePlatformex(this IApplicationBuilder app)
        {
            app.UseSwagger();

            var options = app.ApplicationServices.GetRequiredService<PlatformexOpenApiOptions>();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/" + options.Url.Trim('/') + "/v1/swagger.json", options.Name);

                //c.OAuthClientId("swaggerui");
                //c.OAuthAppName("Swagger UI");
            });

            app.UseMiddleware<PlatformexMiddleware>();
            
            if (app.ApplicationServices.GetService(typeof(EventFlyGraphQlOptions)) is EventFlyGraphQlOptions optionsGraphQl)
                app.UseGraphQL<ISchema>("/" + optionsGraphQl.BasePath.Trim('/'));

            if (app.ApplicationServices.GetService(typeof(EventFlyGraphQlConsoleOptions)) is EventFlyGraphQlConsoleOptions optionsConsole)
                app.UseGraphQLPlayground(new GraphQLPlaygroundOptions
                {
                    Path = "/" + optionsConsole.BasePath.Trim('/')
                });
            return app;
        }
        public static PlatformBuilder ConfigureGraphQl(this PlatformBuilder builder, Action<EventFlyGraphQlOptions> optionsBuilder)
        {
            var options = new EventFlyGraphQlOptions("graphql");

            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
                services.AddSingleton<IDocumentWriter, DocumentWriter>();

                services.AddSingleton<Root>();
                services.AddSingleton<ISchema, GraphSchemaInternal>();
                services.AddGraphQL(_ =>
                {
                    _.EnableMetrics = true;
                    _.ExposeExceptions = true;
                });

                foreach (var query in builder.Definitions.Queries)
                {
                    var handlerType = typeof(GraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    var handlerFullType = typeof(IGraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, query.Value.ResultType);
                    services.AddSingleton(handlerFullType, handlerType);
                    //services.AddSingleton(provider => (IGraphQueryHandler) provider.GetService(handlerFullType));
                }

            });
            return builder;
        }

        public static PlatformBuilder WithConsole(this PlatformBuilder builder, Action<EventFlyGraphQlConsoleOptions> builderOptions)
        {
            var options = new EventFlyGraphQlConsoleOptions("graphql-console");
            builderOptions(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
            });

            return builder;
        }

        //public static IApplicationBuilder UseEventFlyGraphQl(this IApplicationBuilder app)
        //{
        //    var options = app.ApplicationServices.GetRequiredService<EventFlyGraphQlOptions>();
        //    var optionsConsole = app.ApplicationServices.GetRequiredService<EventFlyGraphQlConsoleOptions>();


        //    app.UseGraphQL<ISchema>("/" + options.BasePath.Trim('/'));
        //    app.UseGraphQLPlayground(new GraphQLPlaygroundOptions
        //    {
        //        Path = "/" + optionsConsole.BasePath.Trim('/')
        //    });
        //    return app;
        //}
    }
}
