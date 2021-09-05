using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IServiceFixtureArranger<TServiceInterface, TService>
        where TService : ServiceBase, TServiceInterface
        where TServiceInterface : IService
    {
        IServiceFixtureArranger<TServiceInterface, TService> For();
        IServiceFixtureExecutor<TServiceInterface, TService> GivenNothing();
        IServiceFixtureExecutor<TServiceInterface, TService> GivenMetadata(ServiceMetadata metadata);
    }
}