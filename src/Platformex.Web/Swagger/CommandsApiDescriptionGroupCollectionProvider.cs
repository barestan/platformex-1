using System;
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

        private ApiDescriptionGroupCollection _apiDescriptionGroups;
        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                if (_apiDescriptionGroups != null) return _apiDescriptionGroups;

                var descriptionGroups = _internal.ApiDescriptionGroups;
                var apis = new List<ApiDescription>();
                PrepareCommands(apis);
                PrepareServices(apis,descriptionGroups);
                _apiDescriptionGroups = new ApiDescriptionGroupCollection(PrepareQueries(apis, descriptionGroups), 1);
                return _apiDescriptionGroups;
            }
        }

        private List<ApiDescriptionGroup> PrepareQueries(List<ApiDescription> apis, ApiDescriptionGroupCollection data)
        {
            var contexts = _platform.Definitions.Queries.Select(i => (GetDomainName(i.Value), i.Value))
                .GroupBy(i=>i.Item1).ToList();

            foreach (var domain in contexts)
            {
                foreach (var query in domain.Select(i=>i.Value))
                {
                    apis.Add(CreateApiDescription("Queries", query.QueryType,
                        query.Name, query.QueryType,"POST", 
                        query.QueryType.GetProperties().Select(p=>(p.Name, p.PropertyType)).ToList(),
                        query.ResultType));
                }
            }
            var descriptionGroupList = new List<ApiDescriptionGroup> { new ApiDescriptionGroup("Platformex", apis) };
            descriptionGroupList.AddRange(data.Items);
            return descriptionGroupList;
        }

        private void PrepareServices(List<ApiDescription> apis, ApiDescriptionGroupCollection data)
        {
            var contexts = _platform.Definitions.Services.Select(i => (GetDomainName(i.Value), i.Value))
                .GroupBy(i=>i.Item1).ToList();

            foreach (var domain in contexts)
            {
                foreach (var service in domain.Select(i => i.Value))
                {
                    var type = service.ServiceType;
                    var serviceInterface = type.GetInterfaces()
                        .FirstOrDefault(i => typeof(IService).IsAssignableFrom(i));

                    foreach (var methodInfo in service.GetMethods())
                    {
                        apis.Add(CreateApiDescription(GetDomainName(serviceInterface), service.ServiceType,
                            methodInfo.name, serviceInterface, "PUT", 
                            methodInfo.parameters.Select(p=>(p.Name, p.ParameterType)).ToList(),
                            typeof(Result)));
                    }
                }
            }
            var descriptionGroupList = new List<ApiDescriptionGroup> { new ApiDescriptionGroup("Platformex", apis) };
            descriptionGroupList.AddRange(data.Items);
        }

        private void PrepareCommands(List<ApiDescription> apis)
        {
            var contexts = _platform.Definitions.Commands.Select(i => (GetDomainName(i.Value), i.Value))
                .GroupBy(i=>i.Item1).ToList();

            foreach (var domain in contexts)
            {
                foreach (var allDefinition in domain.Select(i=>i.Value))
                {
                    apis.Add(CreateApiDescription(domain.Key, allDefinition.CommandType,
                        allDefinition.Name, allDefinition.CommandType, "PUT", 
                        allDefinition.CommandType.GetProperties()
                            .Select(i=>(i.Name, i.PropertyType)).ToList(),
                        typeof(Result)));
                }
            }
        }

        private ApiDescription CreateApiDescription(string controllerName, Type controllerType , string methodName, 
            Type methodType, string method, ICollection<(string name, Type type)> parameters, Type resultType)
        {
            var str = _options.BasePath.Trim('/') + "/" + controllerName + "/" + methodName;
            var apiDescription = new ApiDescription();
            var actionDescriptor = new ControllerActionDescriptor
            {
                ActionConstraints =
                    new List<IActionConstraintMetadata>
                    {
                        new HttpMethodActionConstraint(new[] {method})
                    },
                ActionName = methodName,
                ControllerName = controllerName,
                DisplayName = methodName,
                Parameters = parameters
                    .Select(p=> new ParameterDescriptor { Name = p.name ?? "", ParameterType = p.type}).ToList(),
                MethodInfo = new CustomMethodInfo(methodName, methodType),
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                RouteValues = new Dictionary<string, string> {{"controller", controllerName}}
            };
            apiDescription.ActionDescriptor = actionDescriptor;
            apiDescription.SupportedRequestFormats.Add(new ApiRequestFormat
            {
                MediaType = "application/json"
            });
            apiDescription.HttpMethod = method;
            apiDescription.RelativePath = str;

            if (resultType == typeof(Result))
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = 200
                });
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = 422,
                    Type = typeof(string),
                    ApiResponseFormats = new List<ApiResponseFormat>
                    {
                        new ApiResponseFormat
                        {
                            MediaType = "application/json"
                        }
                    },
                    ModelMetadata = _metadataProvider.GetMetadataForType(typeof(string))
                });
            }
            else
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = 200,
                    Type = resultType,
                    ApiResponseFormats = new List<ApiResponseFormat>
                    {
                        new ApiResponseFormat
                        {
                            MediaType = "application/json"
                        }
                    },
                    ModelMetadata = _metadataProvider.GetMetadataForType(resultType)
                });
            }

            apiDescription.SupportedResponseTypes.Add(new ApiResponseType
            {
                StatusCode = 401
            });
            apiDescription.SupportedResponseTypes.Add(new ApiResponseType
            {
                StatusCode = 403
            });

            foreach (var parameter in parameters)
            {
                if (parameter.name == "Metadata") continue; 
                var type = typeof(IIdentity).IsAssignableFrom(parameter.type) ? typeof(string) : parameter.type ?? typeof(string);

                ((List<ApiParameterDescription>)apiDescription.ParameterDescriptions).Add(new ApiParameterDescription
                {
                    Name = parameter.name,
                    Type = type,
                    Source = BindingSource.Form,
                    ModelMetadata = _metadataProvider.GetMetadataForType(type),
                    IsRequired = true,
                });
                
            }
            return apiDescription;
        }
        private string GetDomainName(QueryDefinition definition) 
            => definition.QueryType.Namespace?.Split(".").LastOrDefault()?.Replace("Context","");

        private string GetDomainName(CommandDefinition definition)
            => definition.CommandType.Namespace?.Split(".").LastOrDefault()?.Replace("Context","");

        private string GetDomainName(ServiceDefinition definition)
            => definition.ServiceType.Namespace?.Split(".").LastOrDefault()?.Replace("Context","");

        private string GetDomainName(Type definition)
            => definition.Namespace?.Split(".").LastOrDefault()?.Replace("Context","");

    }
}
