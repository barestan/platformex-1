using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Platformex.Tests
{
    public static class LogHelper
    {
        public static ILogger Logger { get; }

        private static readonly IServiceProvider ServiceProvider;

        static LogHelper()
        {
            ServiceProvider = new ServiceCollection()
                .AddLogging(b => b.AddConsole())
                .BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<object>>();
        }

        public static ILogger<T> For<T>() => ServiceProvider.GetRequiredService<ILogger<T>>();
    }
}