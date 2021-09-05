using Platformex.Domain;

namespace Platformex.Tests
{
    public interface ISubscriberFixtureArranger<TSubscriber,TIdentity, in TEvent>
        where TSubscriber : Subscriber<TIdentity, TEvent>
        where TIdentity : Identity<TIdentity> 
        where TEvent : IAggregateEvent<TIdentity>
    {
        ISubscriberFixtureArranger<TSubscriber,TIdentity, TEvent> For();
        ISubscriberFixtureExecutor<TSubscriber,TIdentity, TEvent> GivenNothing();
    }
}