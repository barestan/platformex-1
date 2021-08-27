using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Siam.MemoContext.Domain
{
    public interface IMemoState : IAggregateState<MemoId>
    {
        MemoDocument Document { get; }
        MemoStatus Status { get; }
        IEnumerable<MemoStatusHistory> History { get; }
    }
    [Description("Памятка")]
    public class MemoAggregate : Aggregate<MemoId, IMemoState, MemoAggregate>, IMemo
    {
        public async Task<CommandResult> Do(UpdateMemo command)
        {
            await Emit(new MemoUpdated(State.Id, command.Document));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(SignMemo command)
        {
            await Emit(new SigningStarted(State.Id, command.UserId));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(ConfirmSigningMemo command)
        {
            await Emit(new MemoSigned(State.Id));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(RejectMemo command)
        {
            await Emit(new RejectionStarted(State.Id, command.UserId, command.RejectionReason));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(ConfirmRejectionMemo command)
        {
            await Emit(new MemoRejected(State.Id));
            return CommandResult.Success;
        }
    }
}