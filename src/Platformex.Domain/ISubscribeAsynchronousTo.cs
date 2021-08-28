using System.Threading.Tasks;
using Orleans;

namespace Platformex.Domain
{
    public interface ISubscriber<in TIdentity, in TEvent> : IGrainWithStringKey
        where TIdentity : Identity<TIdentity>
        where TEvent : IAggregateEvent<TIdentity>
    {
        Task HandleAsync(IDomainEvent<TIdentity, TEvent> domainEvent);
    }
}