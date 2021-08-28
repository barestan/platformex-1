using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace Platformex.Domain
{
    public abstract class Job : Grain, IRemindable, IJob
    {

        public abstract Task ExecuteAsync();

        protected abstract Task Initialize();

        private ILogger _logger;
        protected ILogger Logger => GetLogger();
        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        protected virtual string GetPrettyName() => $"{GetJobName()}:{this.GetPrimaryKeyString()}";
        protected virtual string GetJobName() => GetType().Name.Replace("Job", "");

        public override async Task OnActivateAsync()
        {
            Logger.LogInformation($"(Job [{GetJobName()}] activating...");
            var streamProvider = GetStreamProvider("EventBusProvider");

            //Игнорируем инициализирующее событие 
            await streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions")
                .SubscribeAsync((_, _) => Task.CompletedTask);
            await Initialize();
            await base.OnActivateAsync();

            Logger.LogInformation($"(Job [{GetJobName()}] activated.");
        }

        protected async Task RegisterOrUpdateJob(TimeSpan dueTime, TimeSpan period)
        {
            await RegisterOrUpdateReminder(GetType().Name, dueTime, period);
            Logger.LogInformation($"(Job [{GetJobName()}] registered dueTime:{dueTime} period:{period}.");
        }

        protected async Task UnregisterJob()
        {
            var reminder = await GetReminder(GetType().Name);
            if (reminder != null)
                await UnregisterReminder(reminder);

            Logger.LogInformation($"(Job [{GetJobName()}] unregistered.");
        }

        protected Task<CommandResult> ExecuteAsync<TIdentity>(ICommand<TIdentity> command) 
            where TIdentity : Identity<TIdentity>
        {
            var platform = ServiceProvider.GetService<IPlatform>();
            return platform?.ExecuteAsync(command.Id.Value, command);
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            Logger.LogInformation($"(Job [{GetJobName()}] executing...");

            try
            {
                await ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in Job [{GetJobName()}: {ex.Message}", ex);
                return;
            }

            Logger.LogInformation($"(Job [{GetJobName()}] executed");
        }
        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"(Job [{GetPrettyName()}] deactivated.");

            return base.OnDeactivateAsync();
        }

    }
}