using Microsoft.Extensions.Logging;
using Orleans.TestKit;
using Platformex.Infrastructure;

namespace Platformex.Tests
{
    public abstract class PlatformexTestKit : TestKitBase
    {
        internal PlatformBuilder Builder { get; }
        internal TestPlatform Platform { get; }
        internal TestKitSilo TestKitSilo => Silo;

        protected PlatformexTestKit()
        {
            var loggerFactory = new LoggerFactory();
            Silo.AddService<ILoggerFactory>(loggerFactory);

            Platform = new TestPlatform();
            Builder = new PlatformBuilder(Platform); 
            Silo.AddService<IPlatform>(Platform);
           
        }
    }
}