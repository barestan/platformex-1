using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISagaFixtureExecutor<TSaga, out TSagaState>
        where TSaga : Saga<TSagaState, TSaga> where TSagaState : ISagaState
    {
        ISagaFixtureAsserter<TSaga, TSagaState> When<TIdentity, TAggregateEvent>(TAggregateEvent @event, 
            Result[] commandResults = null, EventMetadata metadata = null)
            where TAggregateEvent : IAggregateEvent<TIdentity> where TIdentity : Identity<TIdentity>;

    }
}