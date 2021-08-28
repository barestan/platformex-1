using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platformex;
using Platformex.Domain;
using Siam.MemoContext;

namespace Siam.Application
{
    [Subscriber]
    public class AutoRejectMemoSaga : StatelessSaga<AutoRejectMemoSaga>,
        IStartedBy<MemoId,RejectionStarted>,
        ISubscribeTo<MemoId,MemoRejected>
    {

        public string Test;
        public async Task<string> HandleAsync(IDomainEvent<MemoId, RejectionStarted> domainEvent)
        {
            await ExecuteAsync(new ConfirmRejectionMemo(domainEvent.AggregateIdentity));
            Test = "100";
            return domainEvent.AggregateEvent.Id.Value;
        }

        public Task HandleAsync(IDomainEvent<MemoId, MemoRejected> domainEvent)
        {
            Logger.LogInformation("ОК!");
            return Task.CompletedTask;
        }
    }
}