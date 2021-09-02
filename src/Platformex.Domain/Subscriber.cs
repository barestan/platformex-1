using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;

namespace Platformex.Domain
{
    [Reentrant]
    public abstract class Subscriber<TIdentity, TEvent> : Grain, ISubscriber<TIdentity, TEvent> 
        where TIdentity : Identity<TIdentity> 
        where TEvent : IAggregateEvent<TIdentity>
    {
        protected bool IsSync { get; }

        public abstract Task HandleAsync(IDomainEvent<TIdentity, TEvent> domainEvent);

        protected Subscriber(bool isSync = false)
        {
            IsSync = isSync;
        }

        private ILogger _logger;
        protected ILogger Logger => GetLogger();
        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        protected virtual string GetPrettyName() => $"{GetSubscriberName()}:{this.GetPrimaryKeyString()}";
        protected virtual string GetSubscriberName() => GetType().Name.Replace("Job", "");
        protected SecurityContext SecurityContext { get; private set; }

        public override async Task OnActivateAsync()
        {
            Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] activating...");
            var streamProvider = GetStreamProvider("EventBusProvider");

            //Игнорируем инициализирующее событие 
            await streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions")
                .SubscribeAsync((_, _) => Task.CompletedTask);


            var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, 
                StreamHelper.EventStreamName(typeof(TEvent),IsSync));

            //Подписываемся на события
            await eventStream.SubscribeAsync(async (data, _) =>
            {
                Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] received event {data.GetPrettyName()}.");
            
                var sc = new SecurityContext(data.Metadata);
                //Проверим права
                var requiredUser = SecurityContext.IsUserRequiredFrom(data);
                if (requiredUser && !sc.IsAuthorized)
                    throw new UnauthorizedAccessException();
            
                var requiredRole = SecurityContext.GetRolesFrom(data);
                if (requiredRole != null)
                    sc.HasRoles(requiredRole);

                SecurityContext = sc;

                //Вызываем сагу для обработки события
                if (IsSync)
                {
                    Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] handling event sync {data.GetPrettyName()}...");
                    
                    try
                    {
                        await HandleAsync((IDomainEvent<TIdentity, TEvent>) data).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error in Subscriber [{GetSubscriberName()}: {ex.Message}", ex);
                        return;
                    }
                    Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] handle event {data.GetPrettyName()}.");
                }
                else
                {
                    Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] handling event async {data.GetPrettyName()}...");
                    try
                    {
                        var __ = HandleAsync((IDomainEvent<TIdentity, TEvent>) data).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error in Subscriber [{GetSubscriberName()}: {ex.Message}", ex);
                        return;
                    }
                    Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] handle event {data.GetPrettyName()}.");
                }

            });


            await base.OnActivateAsync();

            Logger.LogInformation($"(Subscriber [{GetSubscriberName()}] activated.");
        }
        protected Task<Result> ExecuteAsync<T>(ICommand<T> command) 
            where T : Identity<T>
        {
            var platform = ServiceProvider.GetService<IPlatform>();
            return platform?.ExecuteAsync(command.Id.Value, command);
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"(Subscriber [{GetPrettyName()}] deactivated.");

            return base.OnDeactivateAsync();
        }
    }
}