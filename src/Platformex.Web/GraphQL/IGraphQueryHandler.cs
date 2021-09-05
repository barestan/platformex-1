using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Execution;
using GraphQL.Types;

namespace Platformex.Web.GraphQL
{

    public interface IGraphQueryHandler
    {
        FieldType GetFieldType(bool isInput);
        IGraphType GetQueryItemType(Type modelType, bool isInput);
        Task<object> ExecuteQuery(Dictionary<string, ArgumentValue> arguments);
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IGraphQueryHandler<in TQuery, TResult> : IGraphQueryHandler where TQuery : IQuery<TResult>
    {
    }

}
