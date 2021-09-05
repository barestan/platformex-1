using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GraphQL.Types;
using Type = System.Type;

namespace Platformex.Web.GraphQL
{
    internal sealed class ObjectGraphTypeFromModel : ObjectGraphType<object>
    {

        public ObjectGraphTypeFromModel(Type modelType, IGraphQueryHandler graphQueryHandler, bool isInput)
        {
            var modelType1 = modelType;
            IsTypeOf = type => type.GetType().IsAssignableFrom(modelType1);

            var name = !modelType1.Name.EndsWith("Model")
                ? modelType1.Name
                : modelType1.Name.Substring(0, modelType1.Name.Length - "Model".Length);
            Name = name.Replace("`", "");
            Description = modelType1.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var fields = QueryParametersHelper.GetFields(modelType1, graphQueryHandler, isInput);
            foreach (var field in fields)
            {
                AddField(field);
            }
        }
    }
    internal sealed class ObjectGraphTypeFromDomain : ObjectGraphType<object>
    {

        public ObjectGraphTypeFromDomain(string domainName ,List<QueryDefinition> queries, IServiceProvider provider)
        {
            Name = domainName;
            //Description = modelType1.GetCustomAttribute<DescriptionAttribute>()?.Description;

            foreach (var query in queries)
            {
                var gQueryType = typeof(IGraphQueryHandler<,>).MakeGenericType(query.QueryType, query.ResultType);
                var handler = (IGraphQueryHandler)provider.GetService(gQueryType);

                if (handler != null) AddField(handler.GetFieldType(false));
            }
        }

    }


    internal sealed class InputObjectGraphTypeFromModel : InputObjectGraphType<object>
    {

        public InputObjectGraphTypeFromModel(Type modelType, IGraphQueryHandler graphQueryHandler)
        {
            var modelType1 = modelType;
            //IsTypeOf = type => type.GetType().IsAssignableFrom(modelType1);

            Name = "Input" + (!modelType1.Name.EndsWith("Model") ? modelType1.Name : modelType1.Name.Substring(0, modelType1.Name.Length - "Model".Length));
            Description = modelType1.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var fields = QueryParametersHelper.GetFields(modelType1, graphQueryHandler, true);
            foreach (var field in fields)
            {
                AddField(field);
            }
        }
    }

}