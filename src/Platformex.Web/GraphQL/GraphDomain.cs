using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{
    public sealed class GraphDomain
    {
        private readonly IPlatform _platform;
        private readonly IServiceProvider _provider;
        private readonly Dictionary<string, Type> _handlers = new Dictionary<string, Type>();

        public GraphDomain(IPlatform platform, IServiceProvider provider)
        {
            _platform = platform;
            _provider = provider;
            foreach (var query in _platform.Definitions.Queries)
            {
                var name = query.Value.QueryType.Name;
                name = !name.EndsWith("Query") ? name : name.Substring(0, name.Length - "Query".Length);
                _handlers.Add(name, typeof(IGraphQueryHandler<,>).MakeGenericType(query.Value.QueryType, 
                    query.Value.ResultType));
            }
        }

        public FieldType GetFieldType()
        {
            return new FieldType
            {
                ResolvedType = new ObjectGraphTypeFromDomain("Queries", 
                    _platform.Definitions.Queries.Select(i=>i.Value).ToList(), 
                    _provider),
                Name = GetDomainName("Queries"),
                //Description = _domainDefinition.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description,
                Arguments = new QueryArguments(),
                Resolver = new FuncFieldResolver<object>(Execute),
            };
        }

        private string GetDomainName(string name)
        {
            return !name.EndsWith("Context") ? name : name.Substring(0, name.Length - "Context".Length);
        }


        private Task<object> Execute(IResolveFieldContext context)
        {
            if (context.FieldAst.SelectionSet == null) return null;

            foreach (var field in context.FieldAst.SelectionSet.Children.Cast<Field>())
            {
                var queryName = field.Name;

                if (queryName.StartsWith("__")) continue;

                var handler = GetQueryHandler(queryName);
                var f = handler.GetFieldType(true);

                var args = ExecutionHelper.GetArgumentValues(f.Arguments, field.Arguments, new Variables());
                return handler.ExecuteQuery(args);
            }

            return null;
        }


        private IGraphQueryHandler GetQueryHandler(string queryName)
        {
            return (IGraphQueryHandler)_provider.GetService(
                _handlers.FirstOrDefault(i => i.Key.Equals(queryName, StringComparison.InvariantCultureIgnoreCase)).Value);
        }
    }
}