using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Platformex.Application;

namespace Platformex.Infrastructure
{
    public class PlatformBuilder
    {
        private readonly IPlatform _platform;
        internal List<Action<IServiceCollection>> ConfigureServicesActions = new List<Action<IServiceCollection>>();
        internal List<Func<IServiceProvider, Task>> ConfigureStartupActions = new List<Func<IServiceProvider, Task>>();

        public PlatformBuilder(IPlatform platform)
        {
            _platform = platform;
        }

        public Definitions Definitions => _platform.Definitions;

        public PlatformBuilder RegisterApplicationParts<T>()
        {
            var asm = typeof(T).Assembly;
            _platform.Definitions.RegisterApplicationParts(asm);
            WithQueries(asm);
            WithServices(asm);
            return this;
        }

        public void AddConfigureServicesActions(Action<IServiceCollection> action)
        {
            ConfigureServicesActions.Add(action);
        }

        public void AddStartupActions(Func<IServiceProvider, Task> action)
        {
            ConfigureStartupActions.Add(action);
        }
        private void WithQueries(Assembly assembly)
        {
            //TODO: Рефакторнг
            foreach (var type in assembly.GetTypes())
            {
                var queryInterface = type.GetInterfaces().FirstOrDefault(j => j.IsGenericType
                    && j.GetGenericTypeDefinition() ==
                    typeof(IQuery<>));
                if (queryInterface == null) continue;
                
                var resultType = queryInterface.GetGenericArguments()[0];
                RegisterQuery(type,resultType);
            }
        }

        private void WithServices(Assembly assembly)
        {
            //TODO: Рефакторнг
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetInterfaces().Contains(typeof(IDomainService)))
                {
                    RegisterService(type);
                }
            }
        }

        public void RegisterCommand(Type tIdentity, Type tCommand)
        {
            _platform.Definitions.Register(new CommandDefinition(
                tCommand.Name.Replace("Command",""), 
                tIdentity, 
                tCommand, 
                tCommand.GetCustomAttribute<PublicAttribute>() != null
            ));
        }
        
        public void RegisterCommand<TIdentity, TCommand>()
            where TIdentity : Identity<TIdentity>
            where TCommand : class, ICommand<TIdentity>
        {
            RegisterCommand(typeof(TIdentity), typeof(TCommand));
        }

        public void RegisterService(Type tService)
        {
            _platform.Definitions.Register(new ServiceDefinition(
                tService.Name.Replace("Service",""), 
                tService,
                tService.GetCustomAttribute<PublicAttribute>() != null
            ));
        }

        public void RegisterQuery(Type tQuery, Type tResult)
        {
            _platform.Definitions.Register(new QueryDefinition(
                tQuery.Name.Replace("Query",""), 
                tQuery,
                tResult,
                tQuery.GetCustomAttribute<PublicAttribute>() != null
            ));
        }
        
        public void RegisterQuery<TQuery, TQueryType>()
            where TQuery : IQuery<TQueryType>
        {
            RegisterQuery(typeof(TQuery), typeof(TQueryType));
        }

        public AggregateBuilder RegisterAggregate<TIdentity, TAggregate, TState>()
            where TIdentity : Identity<TIdentity>
            where TAggregate : class, IAggregate<TIdentity>
            where TState : AggregateState<TIdentity, TState>
        {
            var aggregateInterfaceType = typeof(TAggregate).GetInterfaces()
                .First(i => i.GetInterfaces().Any(j=> j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregate<>)));
            var info = new AggregateDefinition(typeof(TIdentity), typeof(TAggregate),
                aggregateInterfaceType, typeof(TState));

            _platform.Definitions.Register(info);

            WithServices(typeof(TAggregate).Assembly);
            WithQueries(typeof(TAggregate).Assembly);
            return new AggregateBuilder(this, info);
        }

    }

    public class AggregateBuilder
    {
        private readonly PlatformBuilder _platformBuilder;
        private readonly AggregateDefinition _aggregateDefinition;

        internal AggregateBuilder(PlatformBuilder platformBuilder, AggregateDefinition aggregateDefinition)
        {
            _platformBuilder = platformBuilder;
            _aggregateDefinition = aggregateDefinition;
        }

        public void WithCommands()
        {
            var assembles = _aggregateDefinition.AggregateType.Assembly.GetReferencedAssemblies()
                .Select(i=>i.ToString()).ToList();
            assembles.Add(_aggregateDefinition.AggregateType.Assembly.GetName().ToString());
            
            //TODO: Рефакторнг
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (!assembles.Contains(asm.GetName().ToString())) continue;
                foreach (var type in asm.GetTypes())
                {
                    var commandInterface = type.GetInterfaces().FirstOrDefault(j => j.IsGenericType
                                                                         && j.GetGenericTypeDefinition() ==
                                                                         typeof(ICommand<>));
                    if (commandInterface == null) continue;
                    
                    var identityType = commandInterface.GetGenericArguments()[0];
                    if (identityType != _aggregateDefinition.IdentityType) continue;

                    _platformBuilder.RegisterCommand(identityType, type);
                }
            }
        }
    }
}