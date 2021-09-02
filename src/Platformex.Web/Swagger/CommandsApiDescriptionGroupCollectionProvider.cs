using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Platformex.Web.Swagger
{
    public class CommandsApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly ApiDescriptionGroupCollectionProvider _internal;
        private readonly PlatformexWebApiOptions _options;
        private readonly IPlatform _platform;
        private readonly IModelMetadataProvider _metadataProvider;

        public CommandsApiDescriptionGroupCollectionProvider(
          PlatformexWebApiOptions options,
          IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
          IPlatform platform,
          IModelMetadataProvider metadataProvider,
          IEnumerable<IApiDescriptionProvider> apiDescriptionProviders)
        {
            _options = options;
            _platform = platform;
            _metadataProvider = metadataProvider;
            _internal = new ApiDescriptionGroupCollectionProvider(actionDescriptorCollectionProvider, apiDescriptionProviders);
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                var descriptionGroups = _internal.ApiDescriptionGroups;
                var apis = new List<ApiDescription>();
                PrepareCommands(apis);
                return new ApiDescriptionGroupCollection(PrepareQueries(apis, descriptionGroups), 1);
            }
        }

        private List<ApiDescriptionGroup> PrepareQueries(
          List<ApiDescription> apis,
          ApiDescriptionGroupCollection data)
        {
            var contexts = _platform.Definitions.Queries.Select(i => (GetDomainName(i.Value), i.Value))
                .GroupBy(i=>i.Item1).ToList();
            foreach (var domain in contexts)
            {
                foreach (var query in domain.Select(i=>i.Value))
                {
                    var type = query.QueryType;
                    var name = query.Name;
                    var str = _options.BasePath.Trim('/') + "/" + query.Name;
                    var genericInterface = ReflectionExtensions.GetSubclassOfRawGenericInterface(typeof(IQuery<>), type);
                    if (!(genericInterface == null))
                    {
                        var genericArgument = genericInterface.GetGenericArguments()[0];
                        var apiDescription1 = new ApiDescription();
                        var apiDescription2 = apiDescription1;
                        var actionDescriptor1 = new ControllerActionDescriptor
                        {
                            ActionConstraints =
                                new List<IActionConstraintMetadata>
                                {
                                    new HttpMethodActionConstraint(new[] {"POST"})
                                },
                            ActionName = name,
                            ControllerName = domain.Key,
                            DisplayName = name,
                            Parameters =
                                new List<ParameterDescriptor>
                                {
                                    new() {Name = "query", ParameterType = type}
                                },
                            MethodInfo = new CustomMethodInfo(name, type),
                            ControllerTypeInfo = type.GetTypeInfo(),
                            RouteValues = new Dictionary<string, string> {{"controller", domain.Key}}
                        };



                        var actionDescriptor2 = actionDescriptor1;
                        apiDescription2.ActionDescriptor = actionDescriptor2;
                        apiDescription1.HttpMethod = "POST";
                        apiDescription1.RelativePath = str;
                        apiDescription1.SupportedRequestFormats.Add(new ApiRequestFormat
                        {
                            MediaType = "application/json"
                        });
                        apiDescription1.SupportedResponseTypes.Add(new ApiResponseType
                        {
                            StatusCode = 200,
                            Type = genericArgument,
                            ApiResponseFormats = new List<ApiResponseFormat>
                            {
                                new ApiResponseFormat
                                {
                                    MediaType = "application/json"
                                }
                            },
                            ModelMetadata = _metadataProvider.GetMetadataForType(genericArgument),
                        });
                        var apiDescription3 = apiDescription1;

                        ((List<ApiParameterDescription>)apiDescription3.ParameterDescriptions).Add(new ApiParameterDescription
                        {
                            Name = "query",
                            Type = type,
                            Source = BindingSource.Body,
                            ModelMetadata = _metadataProvider.GetMetadataForType(type),
                            IsRequired = true,
                        });

                        apis.Add(apiDescription3);
                    }
                }
            }
            var descriptionGroupList = new List<ApiDescriptionGroup> { new ApiDescriptionGroup("Platformex", apis) };
            descriptionGroupList.AddRange(data.Items);
            return descriptionGroupList;
        }

        private string GetDomainName(QueryDefinition definition) 
            => definition.QueryType.Namespace?.Split(".").LastOrDefault();

        private string GetDomainName(CommandDefinition definition)
            => definition.CommandType.Namespace?.Split(".").LastOrDefault();

        private void PrepareCommands(List<ApiDescription> apis)
        {
            var contexts = _platform.Definitions.Commands.Select(i => (GetDomainName(i.Value), i.Value))
                .GroupBy(i=>i.Item1).ToList();
            foreach (var domain in contexts)
            {
                foreach (var allDefinition in domain.Select(i=>i.Value))
                {
                    var type = allDefinition.CommandType;
                    var name = allDefinition.Name;
                    var str = _options.BasePath.Trim('/') + "/" + allDefinition.Name;
                    var apiDescription1 = new ApiDescription();
                    var apiDescription2 = apiDescription1;
                    var actionDescriptor1 = new ControllerActionDescriptor
                    {
                        ActionConstraints =
                            new List<IActionConstraintMetadata>
                            {
                                new HttpMethodActionConstraint(new[] {"POST"})
                            },
                        ActionName = name,
                        ControllerName = domain.Key,
                        DisplayName = allDefinition.Name,
                        Parameters =
                            new List<ParameterDescriptor>
                            {
                                new ParameterDescriptor {Name = "request", ParameterType = type}
                            },
                        MethodInfo = new CustomMethodInfo(name, type),
                        ControllerTypeInfo = type.GetTypeInfo(),
                        RouteValues = new Dictionary<string, string> {{"controller", domain.Key}}
                    };
                    var actionDescriptor2 = actionDescriptor1;
                    apiDescription2.ActionDescriptor = actionDescriptor2;
                    apiDescription1.SupportedRequestFormats.Add(new ApiRequestFormat
                    {
                        MediaType = "application/json"
                    });
                    apiDescription1.HttpMethod = "PUT";
                    apiDescription1.RelativePath = str;
                    apiDescription1.SupportedResponseTypes.Add(new ApiResponseType
                    {
                        StatusCode = 200,
                        Type = typeof(Result),
                        ApiResponseFormats = new List<ApiResponseFormat>
                        {
                            new ApiResponseFormat
                            {
                                MediaType = "application/json"
                            }
                        },
                        ModelMetadata = _metadataProvider.GetMetadataForType(typeof(Result))
                    });
                    var apiDescription3 = apiDescription1;
                    ((List<ApiParameterDescription>)apiDescription3.ParameterDescriptions).Add(new ApiParameterDescription
                    {
                        Name = "request",
                        Type = type,
                        Source = BindingSource.Body,
                        ModelMetadata = _metadataProvider.GetMetadataForType(type),
                        IsRequired = true,
                    });
                    apis.Add(apiDescription3);
                }
            }
        }
    }
}
