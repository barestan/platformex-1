using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISubscriberFixtureExecutor<TSubscriber,TIdentity, in TEvent>
        where TSubscriber : Subscriber<TIdentity, TEvent>
        where TIdentity : Identity<TIdentity> 
        where TEvent : IAggregateEvent<TIdentity>
    {
        ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent> When(TEvent @event, EventMetadata metadata = null);

    }
}