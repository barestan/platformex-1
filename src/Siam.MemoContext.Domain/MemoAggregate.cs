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
        public async Task<Result> Do(UpdateMemo command)
        {
            if (State.Status != MemoStatus.Undefined)
                return Result.Fail("Не возможно изменить документ");

            await Emit(new MemoUpdated(State.Identity, command.Document));
            return Result.Success;
        }

        public async Task<Result> Do(SignMemo command)
        {
            if (State.Status != MemoStatus.Undefined)
                return Result.Fail("Не возможно начать подписание документа");

            await Emit(new SigningStarted(State.Identity, command.UserId));
            return Result.Success;
        }

        public async Task<Result> Do(ConfirmSigningMemo command)
        {
            if (State.Status != MemoStatus.SigningStarted)
                return Result.Fail("Не возможно подписать документ");

            await Emit(new MemoSigned(State.Identity));
            return Result.Success;
        }

        public async Task<Result> Do(RejectMemo command)
        {
            await Emit(new RejectionStarted(State.Identity, command.UserId, command.RejectionReason));
            return Result.Success;
        }

        public async Task<Result> Do(ConfirmRejectionMemo command)
        {
            await Emit(new MemoRejected(State.Identity));
            return Result.Success;
        }
    }
}