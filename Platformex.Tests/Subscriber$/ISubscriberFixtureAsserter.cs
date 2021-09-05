using System;
using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISubscriberFixtureAsserter<TSubscriber,TIdentity, in TEvent>
        where TSubscriber : Subscriber<TIdentity, TEvent>
        where TIdentity : Identity<TIdentity> 
        where TEvent : IAggregateEvent<TIdentity>
    {
        ISubscriberFixtureAsserter<TSubscriber,TIdentity, TEvent> AndWhen(TEvent @event, EventMetadata metadata = null);

        ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent> ThenExpect<TCommandIdentity, TCommand>(
            Predicate<TCommand> commandPredicate = null)
            where TCommandIdentity : Identity<TCommandIdentity> where TCommand : ICommand<TCommandIdentity>;
    }
}