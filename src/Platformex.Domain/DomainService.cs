using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Platformex.Domain
{
    public abstract class DomainService : Grain, IIncomingGrainCallFilter
    {
        private ILogger _logger;
        protected ServiceMetadata Metadata { get; private set; } = new ServiceMetadata();
        protected ILogger Logger => GetLogger();
        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        protected virtual string GetPrettyName() => $"{GetJobName()}:{this.GetPrimaryKeyString()}";
        protected virtual string GetJobName() => GetType().Name;
        protected SecurityContext SecurityContext { get; private set; }

        public override Task OnActivateAsync()
        {
            Logger.LogInformation($"(DomainService [{GetPrettyName()}] activated.");

            return base.OnDeactivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"(DomainService [{GetPrettyName()}] deactivated.");

            return base.OnDeactivateAsync();
        }

        protected Task<Result> ExecuteAsync<TIdentity>(ICommand<TIdentity> command) 
            where TIdentity : Identity<TIdentity>
        {
            var platform = ServiceProvider.GetService<IPlatform>();
            return platform?.ExecuteAsync(command.Id.Value, command);
        }

        public Task SetMetadata(ServiceMetadata metadata)
        {
            Metadata = metadata;
            return  Task.CompletedTask;
        }
        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.InterfaceMethod.Name != "SetMetdadta")
            {
                Logger.LogInformation($"(DomainService method [{context.InterfaceMethod.Name}] invoking...");
            
                var sc = new SecurityContext(Metadata);
                //Проверим права
                var requiredUser = SecurityContext.IsUserRequiredFrom(this);
                if (requiredUser && !sc.IsAuthorized)
                    throw new UnauthorizedAccessException();
            
                var requiredRole = SecurityContext.GetRolesFrom(this);
                if (requiredRole != null)
                    sc.HasRoles(requiredRole);

                SecurityContext = sc;
                
                try
                {
                    await context.Invoke();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    context.Result = Result.Fail(e.Message);
                    return;
                }
               
                Logger.LogInformation($"(DomainService method [{context.InterfaceMethod.Name}] invoked...");
            }
            else
            {
                await context.Invoke();
            }
        }
    }
}