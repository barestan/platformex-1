using System;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Infrastructure;
using Platformex.Web.Swagger;

namespace Platformex.Web.GraphQL
{
    public sealed class PlatformexGraphQlOptions
    {
        public PlatformexGraphQlOptions(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; set; }
    }

    public sealed class PlatformexGraphQlConsoleOptions
    {
        public PlatformexGraphQlConsoleOptions(string basePath)
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
            
            if (app.ApplicationServices.GetService(typeof(PlatformexGraphQlOptions)) is PlatformexGraphQlOptions optionsGraphQl)
                app.UseGraphQL<ISchema>("/" + optionsGraphQl.BasePath.Trim('/'));

            if (app.ApplicationServices.GetService(typeof(PlatformexGraphQlConsoleOptions)) is PlatformexGraphQlConsoleOptions optionsConsole)
                app.UseGraphQLPlayground("/" + optionsConsole.BasePath.Trim('/'));
            return app;
        }
        public static PlatformBuilder ConfigureGraphQl(this PlatformBuilder builder, Action<PlatformexGraphQlOptions> optionsBuilder)
        {
            var options = new PlatformexGraphQlOptions("graphql");

            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                //services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
                //services.AddSingleton<IDocumentWriter, DocumentWriter>();

                services.AddSingleton<Root>();
                services.AddSingleton<ISchema, GraphSchemaInternal>();
                services.AddGraphQL(_ =>
                    {
                        _.EnableMetrics = true;
                        //_.ExposeExceptions = true;
                    })
                    .AddSystemTextJson()
                    .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = true);
                
                //services.AddTransient<IGraphQLRequestDeserializer, GraphQLRequestDeserializer>();



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

        public static PlatformBuilder WithConsole(this PlatformBuilder builder, Action<PlatformexGraphQlConsoleOptions> builderOptions)
        {
            var options = new PlatformexGraphQlConsoleOptions("graphql-console");
            builderOptions(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
            });

            return builder;
        }
    }
}
