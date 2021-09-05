using System;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{
    internal class GraphSchemaInternal : Schema
    {
        public GraphSchemaInternal(IServiceProvider provider) : base(new CustomServiceProvider(provider))
            /*, type =>
        {
            var result = (IGraphType)provider.GetService(type);
            if (result == null && type.GetGenericTypeDefinition() == typeof(EnumerationGraphType<>))
            {
                //TODO: Refactoring
                return (IGraphType)Activator.CreateInstance(type);
            }

            return null;
        })*/
        {
            var root = (Root) provider.GetService(typeof(Root));
            if (root != null) 
                Query = root;
        }
    }

    internal class CustomServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _provider;

        public CustomServiceProvider(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object GetService(Type serviceType)
        {
            var result = _provider.GetService(serviceType);
            if (result == null && serviceType.GetGenericTypeDefinition() == typeof(EnumerationGraphType<>))
            {
                //TODO: Refactoring
                return (IGraphType)Activator.CreateInstance(serviceType);
            }

            return null;
        }
    }
}