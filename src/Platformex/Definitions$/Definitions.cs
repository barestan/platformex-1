﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

#region hack
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}
}
#endregion


namespace Platformex
{
    public record AggregateDefinition(Type IdentityType, Type AggreagteType, Type InterfaceType, Type StateType);
    public record CommandDefinition(string Name, Type IdentityType, Type CommandType, bool IsPublic);
    public record QueryDefinition(string Name,Type QueryType, Type ResultType, bool IsPublic);

    public sealed class Definitions
    {
        public readonly Dictionary<Type, AggregateDefinition> Aggregates = new();
        
        public readonly Dictionary<string, CommandDefinition> Commands = new();
        public readonly Dictionary<string, QueryDefinition> Queries = new();
        public AggregateDefinition Aggregate<TIdentity>() where TIdentity : Identity<TIdentity> 
            => Aggregates[typeof(TIdentity)];
        private readonly List<Assembly> _applicationPartsAssemlies = new();

        public void Register(AggregateDefinition definition)
        {
            Aggregates.Add(definition.IdentityType, definition);
        }
        public void Register(CommandDefinition definition)
        {
            Commands.Add(definition.Name, definition);
        }
        public void Register(QueryDefinition definition)
        {
            Queries.Add(definition.Name, definition);
        }
        public IEnumerable<Assembly> Assemblies =>
            Aggregates.Values.SelectMany(i => new []
            {
                i.AggreagteType.Assembly,
                i.IdentityType.Assembly,
                i.InterfaceType.Assembly,
                i.StateType.Assembly
            }).Concat(_applicationPartsAssemlies)
              .Distinct();

        public void RegisterApplicationParts(Assembly contextAppliactionParts)
        {
            if (_applicationPartsAssemlies.Contains(contextAppliactionParts)) return;
            
            _applicationPartsAssemlies.Add(contextAppliactionParts);

        }

        public bool TryGetDefinition(string name, out CommandDefinition commandDefinition)
        {
            if (Commands.ContainsKey(name))
            {
                commandDefinition = Commands[name];
                return true;
            }

            commandDefinition = null;
            return false;
        }
    }
}