using System;
using System.Threading.Tasks;
using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IServiceFixtureExecutor<TServiceInterface, TService>
        where TService : ServiceBase, TServiceInterface
        where TServiceInterface : IService   
    {
        IServiceFixtureAsserter<TServiceInterface, TService> When(Func<TServiceInterface, Task<object>> testFunc);

    }
}