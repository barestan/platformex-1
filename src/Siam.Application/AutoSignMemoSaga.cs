using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;
using Siam.MemoContext;

namespace Siam.Application
{
    public class AutoSignMemoSaga : StatelessSaga<AutoSignMemoSaga>,
        IStartedBy<MemoId,MemoUpdated>,
        ISubscribeTo<MemoId,MemoSigned>
    {
        public async Task<string> HandleAsync(IDomainEvent<MemoId, MemoUpdated> domainEvent)
        {
            await ExecuteAsync(new SignMemo(domainEvent.AggregateIdentity, "TestUser"));
            return domainEvent.AggregateEvent.Id.Value;
        }

        public Task HandleAsync(IDomainEvent<MemoId, MemoSigned> domainEvent) 
            => ExecuteAsync(new ConfirmSigningMemo(domainEvent.AggregateIdentity));
    }
}