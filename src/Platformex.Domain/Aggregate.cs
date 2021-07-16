﻿
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;

[assembly:InternalsVisibleTo("Platformex.Infrastructure")]

namespace Platformex.Domain
{
    public abstract class Aggregate<TIdentity, TState, TEventApplier> : Grain, IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        private static readonly IReadOnlyDictionary<Type, Func<TEventApplier, ICommand, Task<CommandResult>>> DoCommands;
        private ICommand _pinnedCommand;

        protected SecurityContext SecurityContext;  
        static Aggregate()
        {
            DoCommands = typeof(TEventApplier).GetAggregateDoMethods<TIdentity, TEventApplier>();
        }
        public async Task<CommandResult> DoAsync(ICommand command)
        {
            if (!DoCommands.TryGetValue(command.GetType(), out var applier))
            {
                throw new MissingMethodException($"missing HandleAsync({command.GetType().Name})");
            }

            BeforeApplyingCommand(command);
            
            var result = await applier((TEventApplier) (object) this, command);
            
            AfterApplyingCommand();
            
            return result;

        }

        private void AfterApplyingCommand()
        {
            _pinnedCommand = null;
            SecurityContext = null;
        }
        
        
        

        private void BeforeApplyingCommand(ICommand command)
        {
            var sc = new SecurityContext(command.Metadata);
            //Проверим права
            var requiredUser = SecurityContext.IsUserRequiredFrom(command);
            if (requiredUser && !sc.IsAuthorized)
                throw new UnauthorizedAccessException();
            
            var requiredRole = SecurityContext.GetRolesFrom(command);
            if (requiredRole != null)
                sc.HasRoles(requiredRole);

            SecurityContext = sc;
            _pinnedCommand = command;
        }


        public TIdentity AggregateId => State?.Id ?? this.GetId<TIdentity>();
        protected TState State { get; private set;}

        private IPlatform _platform;

        private ILogger _logger;
        protected ILogger Logger => GetLogger();

        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        protected virtual string GetAggregateName() => GetType().Name.Replace("Aggregate", "");
        protected string GetPrettyName() => $"{GetAggregateName()}:{this.GetPrimaryKeyString()}";

        public override async Task OnActivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activating...");
            try
            {
                _platform = (IPlatform) this.ServiceProvider.GetService(typeof(IPlatform));

                var stateType = _platform.Definitions.Aggregate<TIdentity>()?.StateType;

                if (stateType == null)
                    throw new Exception($"Definitions on aggregate {typeof(TIdentity).Name} not found");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loading...");

                State = this.ServiceProvider.GetService<TState>() ?? Activator.CreateInstance<TState>();
                await State.LoadState(this.GetId<TIdentity>());

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loaded.");

                await base.OnActivateAsync();
            }
            catch (Exception e)
            {
                Logger.LogError($"Aggregate [{GetPrettyName()}] activation error: {e.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
            return base.OnDeactivateAsync();
        }

        protected async Task Emit<TEvent>(TEvent e) where TEvent : class, IAggregateEvent<TIdentity>
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] preparing to emit event {e.GetPrettyName()}...");
            var metadata = CreateEventMetadata(e);
            var domainEvent = new DomainEvent<TIdentity, TEvent>(e.Id, e, DateTimeOffset.Now, 1, 
                metadata);
            try
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changes state ...");
                await State.Apply(e);
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changed state ...");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] fires event {e.GetPrettyName()}...");

                //Посылаем сообщения асинхронно
                var _ = GetStreamProvider("EventBusProvider")
                    .GetStream<IDomainEvent>(Guid.Empty, StreamHelper.EventStreamName(typeof(TEvent),false))
                    .OnNextAsync(domainEvent).ConfigureAwait(false);

                //Посылаем сообщения синхронно
                await GetStreamProvider("EventBusProvider")
                    .GetStream<IDomainEvent>(Guid.Empty, StreamHelper.EventStreamName(typeof(TEvent), true))
                    .OnNextAsync(domainEvent).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] error fires event: {domainEvent.GetPrettyName()} : {ex.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] fired event {e.GetPrettyName()}...");
        }

        private IEventMetadata CreateEventMetadata(IAggregateEvent @event)
        {
            var now = DateTimeOffset.UtcNow;
            var eventId = EventId.NewDeterministic(GuidFactories.Deterministic.Namespaces.Events, $"{AggregateId.Value}-v{now.ToUnixTime()}");
            var eventMetadata = new EventMetadata(_pinnedCommand.Metadata)
            {
                Timestamp = now,
                AggregateSequenceNumber = 0,
                AggregateName = GetAggregateName(),
                AggregateId = AggregateId.Value,
                EventId = eventId,
                EventName = @event.GetPrettyName(),
                EventVersion = 1
            };

            eventMetadata.AddOrUpdateValue(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString());
            return eventMetadata;
        }
    }
}