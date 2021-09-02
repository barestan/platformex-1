using System;
using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISagaFixtureAsserter<TSaga, out TSagaState>
        where TSaga :Saga<TSagaState, TSaga> where TSagaState : ISagaState
    {
        ISagaFixtureAsserter<TSaga, TSagaState> AndWhen<TIdentity,TAggregateEvent>(TAggregateEvent @event,
            Result[] commandResults = null, EventMetadata metadata = null)
            where TAggregateEvent : IAggregateEvent<TIdentity> where TIdentity : Identity<TIdentity>;

        ISagaFixtureAsserter<TSaga, TSagaState> ThenExpect<TIdentity, TCommand>(Predicate<TCommand> commandPredicate = null)
            where TCommand : ICommand<TIdentity> where TIdentity : Identity<TIdentity>;

        ISagaFixtureAsserter<TSaga, TSagaState> ThenExpectState(Predicate<TSagaState> aggregateEventPredicate);
    }
}