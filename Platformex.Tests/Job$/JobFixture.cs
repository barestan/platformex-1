using System;
using System.Collections.Generic;
using Platformex.Domain;
using Xunit;

namespace Platformex.Tests
{
    public class JobFixture<TJob> : IJobFixtureArranger<TJob>,
        IJobFixtureExecutor<TJob>, IJobFixtureAsserter<TJob>
        where TJob : Job
    {
        private readonly PlatformexTestKit _testKit;
        private TJob _job;
        // ReSharper disable once UnusedMember.Local
        private readonly Stack<ICommand> _commands = new Stack<ICommand>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public JobFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            //_testKit.Builder.Register<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public IJobFixtureArranger<TJob> For()
        {
            _testKit.Platform.CommandExecuted += (_, args) =>
            {
                if (_isMonitoring) 
                    _commands.Push(args.Command);
            };
            
            _job = _testKit.TestKitSilo.CreateGrainAsync<TJob>(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            return this;
        }
        public IJobFixtureExecutor<TJob> GivenNothing() => this;



        public IJobFixtureAsserter<TJob> ThenExpect<TCommandIdentity, TCommand>(Predicate<TCommand> commandPredicate = null) 
            where TCommandIdentity : Identity<TCommandIdentity> where TCommand : ICommand<TCommandIdentity>
        {
            if (_commands.Count == 0)
                Assert.True(false, $"Нет ожидаемой команды {typeof(TCommand).Name} ");

            var command = _commands.Pop();
            Assert.True(command.GetType() == typeof(TCommand),
                $"Невалидная комнда, ожидалась {typeof(TCommand).Name} вместо {command.GetType().Name}");
            
            if (commandPredicate != null)
                Assert.True(commandPredicate.Invoke((TCommand)command), $"Невалидая команда {typeof(TCommand).Name}");
            return this;
        }

        public IJobFixtureAsserter<TJob> WhenTimer()
        {
            StartMonitoring();

            _job.ExecuteAsync().GetAwaiter().GetResult();
            
            StopMonitoring();
            return this;
        }
    }
}