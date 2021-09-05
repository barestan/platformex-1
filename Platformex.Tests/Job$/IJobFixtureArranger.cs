using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IJobFixtureArranger<TJob>
        where TJob : Job
    {
        IJobFixtureArranger<TJob> For();
        IJobFixtureExecutor<TJob> GivenNothing();
    }
}