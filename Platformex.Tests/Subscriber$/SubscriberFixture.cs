using System;
using System.Collections.Generic;
using Platformex.Domain;
using Xunit;

namespace Platformex.Tests
{
    public class SubscriberFixture<TSubscriber, TIdentity, TEvent> : ISubscriberFixtureArranger<TSubscriber, TIdentity, TEvent>,
        ISubscriberFixtureExecutor<TSubscriber, TIdentity, TEvent>, ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent>
        where TSubscriber : Subscriber<TIdentity, TEvent>
        where TIdentity : Identity<TIdentity> 
        where TEvent : IAggregateEvent<TIdentity>
    {
        private readonly PlatformexTestKit _testKit;
        private TSubscriber _subscriber;
        // ReSharper disable once UnusedMember.Local
        private readonly Stack<ICommand> _commands = new Stack<ICommand>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public SubscriberFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            //_testKit.Builder.Register<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public ISubscriberFixtureArranger<TSubscriber, TIdentity, TEvent> For()
        {
            _testKit.Platform.CommandExecuted += (_, args) =>
            {
                if (_isMonitoring) 
                    _commands.Push(args.Command);
            };
            
            _subscriber = _testKit.TestKitSilo.CreateGrainAsync<TSubscriber>(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            return this;
        }
        public ISubscriberFixtureExecutor<TSubscriber, TIdentity, TEvent> GivenNothing() => this;


        public ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent> AndWhen(TEvent @event, EventMetadata metadata) 
            => When(@event, metadata);

        public ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent> ThenExpect<TCommandIdentity, TCommand>(Predicate<TCommand> commandPredicate = null) 
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

        public ISubscriberFixtureAsserter<TSubscriber, TIdentity, TEvent> When(TEvent @event, EventMetadata metadata)
        {
            StartMonitoring();

            var domainEvent = new DomainEvent<TIdentity, TEvent>(@event.Id, @event, DateTimeOffset.Now,
                1, metadata ?? EventMetadata.Empty);
            _subscriber.ProcessEventInternal(domainEvent).GetAwaiter().GetResult();
            _testKit.Platform.ClearCommandResults();
            
            StopMonitoring();
            return this;
        }
    }
}