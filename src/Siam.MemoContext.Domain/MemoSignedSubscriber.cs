using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Siam.MemoContext.Domain
{
    [Subscriber]
    public class MemoSigningStartedSubscriber : Subscriber<MemoId, SigningStarted>
    {
        public override async Task HandleAsync(IDomainEvent<MemoId, SigningStarted> domainEvent)
        {
            await ExecuteAsync(new RejectMemo(domainEvent.AggregateEvent.Id, string.Empty, RejectionReason.Undefined));
        }
    }
}