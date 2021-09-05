using Platformex.Domain;

namespace Platformex.Tests
{
    public interface IJobFixtureExecutor<TJob>
        where TJob : Job
    {
        IJobFixtureAsserter<TJob> WhenTimer();

    }
}