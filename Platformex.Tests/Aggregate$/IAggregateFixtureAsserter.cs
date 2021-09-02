using System;
using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IAggregateFixtureAsserter<TAggregate, TIdentity, TState>
        where TAggregate : IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> AndWhen(params ICommand[] commands);

        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpect<TAggregateEvent>(Predicate<TAggregateEvent> aggregateEventPredicate = null)
            where TAggregateEvent : class, IAggregateEvent<TIdentity>;

        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectState(
            Predicate<TState> aggregateEventPredicate = null);
            

        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectResult(Predicate<Result> aggregateReply = null);

        IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectDomainEvent<TAggregateEvent>(Predicate<IDomainEvent<TIdentity, TAggregateEvent>> domainEventPredicate = null)
            where TAggregateEvent : IAggregateEvent<TIdentity>;
    }
}