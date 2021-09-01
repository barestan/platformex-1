using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Platformex.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Platformex.Web.Swagger
{
    public sealed class PlatformexOpenApiOptions
    {
        public PlatformexOpenApiOptions(string url, string name)
        {
            Url = url;
            Name = name;
        }

        public string Url { get; set; }
        public string Name { get; set; }
    }

    public static class BuilderExtensions
    {
        public static PlatformBuilder WithOpenApi(this PlatformBuilder builder, Action<PlatformexOpenApiOptions> optionsBuilder)
        {
            var options = new PlatformexOpenApiOptions("swagger", Assembly.GetEntryAssembly()?.GetName().Name);
            optionsBuilder(options);
            builder.AddConfigureServicesActions(services =>
            {
                services.AddSingleton(options);
                
                services.TryAdd(ServiceDescriptor
                    .Transient<IApiDescriptionGroupCollectionProvider,
                        CommandsApiDescriptionGroupCollectionProvider>());
                            
                services.AddSingleton<IApiDescriptionGroupCollectionProvider,
                    CommandsApiDescriptionGroupCollectionProvider>();

                services.AddSwaggerGen(c =>
                {
                    c.DocInclusionPredicate((docName, apiDesc) =>
                    {
                        if (apiDesc.TryGetMethodInfo(out _))
                        {
                            return apiDesc.HttpMethod != null;
                        }

                        return false;
                    });
                });
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo {Title = options.Name + " API", Version = "v1"});


                    c.OperationFilter<DescriptionFilter>();
                    c.SchemaFilter<ReadOnlyFilter>();
                    c.CustomSchemaIds(i => i.FullName);
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var files = Directory.GetFiles(basePath, "*.xml");
                    foreach (var file in files)
                    {
                        c.IncludeXmlComments(file);
                    }
                    //c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    //{
                    //	Type = "oauth2",
                    //	Flow = "implicit",
                    //	AuthorizationUrl = $"{siteOptions?.Security?.BaseUrl}/connect/authorize",
                    //	TokenUrl = $"{siteOptions?.Security?.BaseUrl}/connect/token",
                    //	Scopes = currentScopes
                    //});
                });
            });
            return builder;
        }

        //public static IApplicationBuilder UseEventFlySwagger(this IApplicationBuilder app)
        //{

        //    app.UseSwagger();
        //    var options = app.ApplicationServices.GetRequiredService<PlatformexOpenApiOptions>();
        //    app.UseSwaggerUI(c =>
        //    {
        //        c.SwaggerEndpoint("/" + options.Url.Trim('/') + "/v1/swagger.json", options.Name);

        //        //c.OAuthClientId("swaggerui");
        //        //c.OAuthAppName("Swagger UI");
        //    });
        //    return app;
        //}

    }

    public class DescriptionFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor desc)
            {
                var actionType = desc.ControllerTypeInfo.AsType();
                bool isCommand = actionType.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(ICommand<>));
                
                bool isQuery = actionType.GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(ICommand<>));
                
                if (isCommand || isQuery)
                {
                    operation.Summary = actionType.GetCustomAttribute<DescriptionAttribute>()?.Description;
                }
            }
        }
    }
    public class ReadOnlyFilter : ISchemaFilter
    {

        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model.Properties == null)
            {
                return;
            }

            foreach (var schemaProperty in model.Properties)
            {
                var property = context.Type.GetProperty(schemaProperty.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    schemaProperty.Value.ReadOnly = false;
                }
            }
        }
    }
}
