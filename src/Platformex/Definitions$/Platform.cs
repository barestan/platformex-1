using System.Threading.Tasks;

namespace Platformex
{
    public interface IDomain
    {
        TAggregate GetAggregate<TAggregate>(string id)
            where TAggregate : IAggregate;

    }
    public interface IPlatform : IDomain
    {
        Definitions Definitions { get; }
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
        Task<object> QueryAsync(IQuery query);
        Task<Result> ExecuteAsync(string aggregateId, ICommand command);
        Task PublishEvent(IDomainEvent domainEvent);
        TDomainService Service<TDomainService>() where TDomainService : IService;
    }
}