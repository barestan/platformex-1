using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Platformex.Application;
using Platformex.Domain;

#region hack
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}
}
#endregion

namespace Platformex.Tests.TestHelpers
{

    // События
    public record ThingyDeletedEvent(ThingyId Id) : IAggregateEvent<ThingyId>;
    public record ThingyDomainErrorAfterFirstEvent(ThingyId Id) : IAggregateEvent<ThingyId>;
    public record ThingyMessageAddedEvent(ThingyId Id, ThingyMessage ThingyMessage) : IAggregateEvent<ThingyId>;
    public record ThingyMessageHistoryAddedEvent(ThingyId Id, ThingyMessage[] ThingyMessages) : IAggregateEvent<ThingyId>;
    public record ThingyPingEvent(ThingyId Id, PingId PingId) : IAggregateEvent<ThingyId>;
    public record ThingySagaCompleteRequestedEvent(ThingyId Id) : IAggregateEvent<ThingyId>;
    public record ThingySagaExceptionRequestedEvent(ThingyId Id) : IAggregateEvent<ThingyId>;
    public record ThingySagaStartRequestedEvent(ThingyId Id) : IAggregateEvent<ThingyId>;

    //Команды
    public record ThingyAddMessageCommand(ThingyId Id, ThingyMessage ThingyMessage) : Command, ICommand<ThingyId>;
    public record ThingyAddMessageHistoryCommand(ThingyId Id, ThingyMessage[] ThingyMessages) : Command, ICommand<ThingyId>;
    public record ThingyDeleteCommand(ThingyId Id, PingId PingId) : Command, ICommand<ThingyId>;
    public record ThingyDomainErrorAfterFirstCommand(ThingyId Id) : Command, ICommand<ThingyId>;
    public record ThingyImportCommand(ThingyId Id, IEnumerable<PingId> PingIds, IEnumerable<ThingyMessage> ThingyMessages) : Command, ICommand<ThingyId>;
    public record ThingyMaybePingCommand(ThingyId Id, PingId PingId, bool IsSuccess) : Command, ICommand<ThingyId>;
    public record ThingyMultiplePingsCommand(ThingyId Id, IEnumerable<PingId> PingIds) : Command, ICommand<ThingyId>;
    public record ThingyNopCommand(ThingyId Id) : Command, ICommand<ThingyId>;
    public record ThingyPingCommand(ThingyId Id, PingId PingId) : Command, ICommand<ThingyId>;
    public record ThingyRequestSagaCompleteCommand(ThingyId Id) : Command, ICommand<ThingyId>;
    public record ThingyRequestSagaStartCommand(ThingyId Id) : Command, ICommand<ThingyId>;
    public record ThingyThrowExceptionInSagaCommand(ThingyId Id) : Command, ICommand<ThingyId>;
   

    public interface IThingyAggregate : IAggregate<ThingyId>,
        ICanDo<ThingyAddMessageCommand, ThingyId>,
        ICanDo<ThingyAddMessageHistoryCommand, ThingyId>,
        ICanDo<ThingyDeleteCommand, ThingyId>,
        ICanDo<ThingyDomainErrorAfterFirstCommand, ThingyId>,
        ICanDo<ThingyImportCommand, ThingyId>,
        ICanDo<ThingyMaybePingCommand, ThingyId>,
        ICanDo<ThingyMultiplePingsCommand, ThingyId>,
        ICanDo<ThingyNopCommand, ThingyId>,
        ICanDo<ThingyPingCommand, ThingyId>,
        ICanDo<ThingyRequestSagaCompleteCommand, ThingyId>,
        ICanDo<ThingyRequestSagaStartCommand, ThingyId>,
        ICanDo<ThingyThrowExceptionInSagaCommand, ThingyId>
    {
    }

    public interface IThingyState : IAggregateState<ThingyId>, IModel
    {
        IReadOnlyCollection<PingId> PingsReceived { get; }
        IReadOnlyCollection<ThingyMessage> Messages { get; }
        bool IsDeleted { get; }
        public bool DomainErrorAfterFirstReceived { get; }

    }

    public class ThingyAggregate : Aggregate<ThingyId, IThingyState,ThingyAggregate>, IThingyAggregate
    {
        public Task<CommandResult> Do(ThingyAddMessageCommand command) 
            => CommandResult.SucceedAsync(() => Emit(new ThingyMessageAddedEvent(command.Id, command.ThingyMessage)));

        public Task<CommandResult> Do(ThingyAddMessageHistoryCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingyMessageHistoryAddedEvent(command.Id, command.ThingyMessages)));

        public Task<CommandResult> Do(ThingyDeleteCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingyDeletedEvent(command.Id)));

        public Task<CommandResult> Do(ThingyDomainErrorAfterFirstCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingyDomainErrorAfterFirstEvent(command.Id)));

        public async Task<CommandResult> Do(ThingyImportCommand command)
        {
            foreach (var pingId in command.PingIds)
            {
                await Emit(new ThingyPingEvent(command.Id, pingId));
            }

            foreach (var thingyMessage in command.ThingyMessages)
            {
                await Emit(new ThingyMessageAddedEvent(command.Id, thingyMessage));
            }

            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(ThingyMaybePingCommand command)
        {
            await Emit(new ThingyPingEvent(command.Id, command.PingId));
            return command.IsSuccess
                ? CommandResult.Success
                : CommandResult.Fail("Error");
        }

        public async Task<CommandResult> Do(ThingyMultiplePingsCommand command)
        {
            foreach (var pingId in command.PingIds)
            {
                await Emit(new ThingyPingEvent(command.Id, pingId));
            }
            return CommandResult.Success;
        }

        public Task<CommandResult> Do(ThingyNopCommand command)
            => Task.FromResult(CommandResult.Success);

        public Task<CommandResult> Do(ThingyPingCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingyPingEvent(command.Id, command.PingId)));

        public Task<CommandResult> Do(ThingyRequestSagaCompleteCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingySagaCompleteRequestedEvent(command.Id)));

        public Task<CommandResult> Do(ThingyRequestSagaStartCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingySagaStartRequestedEvent(command.Id)));

        public Task<CommandResult> Do(ThingyThrowExceptionInSagaCommand command)
            => CommandResult.SucceedAsync(() => Emit(new ThingySagaExceptionRequestedEvent(command.Id)));
    }
}