using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Siam.MemoContext.Domain
{
    [Subscriber]
    public class MemoSigningSubscriber : Subscriber<MemoId, MemoUpdated>
    {
        public override async Task HandleAsync(IDomainEvent<MemoId, MemoUpdated> domainEvent)
        {
            await ExecuteAsync(new SignMemo(domainEvent.AggregateEvent.Id, SecurityContext.UserId));
        }
    }
}