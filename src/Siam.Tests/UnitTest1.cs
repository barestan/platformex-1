using Xunit;
using Orleans.TestKit;
using Platformex.Application;
using Platformex.Domain;
using Platformex.Tests;
using Siam.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;

namespace Siam.Tests
{
    public class UnitTest1 : PlatformexTestKit
    {
        public UnitTest1()
        {
            Silo.AddService<IMemoState>(new MemoState(new InMemoryDbProvider<MemoModel>()));
        }

        [Fact]
        public void TestAggregate()
        {
            var id = MemoId.New;
            var fixture = new AggregateFixture<MemoId, MemoAggregate, IMemoState, MemoState>(this);

            fixture
                .For(id)
                .GivenNothing()
                .When(
                    new RejectMemo(id, string.Empty, RejectionReason.Undefined))
                
                .ThenExpectResult(e => e.IsSuccess)
                .ThenExpectDomainEvent<RejectionStarted>(e 
                    => e.AggregateEvent.Id == id && e.AggregateEvent.RejectionReason == RejectionReason.Undefined)
                .ThenExpectState(s => s.Status == MemoStatus.RejectionStarted);
        }
        [Fact]
        public void TestSaga()
        {
            var id = MemoId.New;
            var fixture = new SagaFixture<AutoRejectMemoSaga, EmptySagaState>(this);

            fixture.For()
                .GivenNothing()
                
                .When<MemoId, MemoUpdated>(new MemoUpdated(id, new MemoDocument()))
                .ThenExpect<MemoId, SignMemo>(command => command.Id == id)
                
                .AndWhen<MemoId, MemoSigned>(new MemoSigned(id))
                .ThenExpect<MemoId, ConfirmSigningMemo>(e => e.Id == id);

        }
        
        
    }
}