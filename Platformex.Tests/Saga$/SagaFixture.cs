using System;
using System.Collections.Generic;
using Platformex.Domain;
using Xunit;

namespace Platformex.Tests
{
    public class SagaFixture<TSaga, TSagaState> : ISagaFixtureArranger<TSaga, TSagaState>,
        ISagaFixtureExecutor<TSaga, TSagaState>, ISagaFixtureAsserter<TSaga, TSagaState>
        where TSaga : Saga<TSagaState, TSaga> where TSagaState : ISagaState
    {
        private readonly PlatformexTestKit _testKit;
        private TSaga _saga;
        // ReSharper disable once UnusedMember.Local
        private TSagaState State => _saga.TestOnlyGetState(); 
        private readonly Stack<ICommand> _commands = new Stack<ICommand>();
        private readonly Stack<IDomainEvent> _events = new Stack<IDomainEvent>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public SagaFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            //_testKit.Builder.Register<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public ISagaFixtureArranger<TSaga, TSagaState> For()
        {
            _testKit.Platform.EventPublished += (_, args) =>
            {
                if (_isMonitoring) 
                    _events.Push(args.DomainEvent);
            };
            _testKit.Platform.CommandExecuted += (_, args) =>
            {
                if (_isMonitoring) 
                    _commands.Push(args.Command);
            };
            
            _saga = _testKit.TestKitSilo.CreateGrainAsync<TSaga>(Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            return this;
        }

        public ISagaFixtureExecutor<TSaga, TSagaState> GivenNothing() => this;


        public ISagaFixtureExecutor<TSaga, TSagaState> Given(params IDomainEvent[] domainEvents)
        {
            foreach (var ent in domainEvents)
            {
                _saga.ProcessEvent(ent).GetAwaiter().GetResult();
            }

            return this;
        }

        public ISagaFixtureExecutor<TSaga, TSagaState> Given(TSagaState aggregateState)
        {
            _saga.TestOnlySetState(aggregateState);
            return this;
        }

        public ISagaFixtureAsserter<TSaga, TSagaState> When<TIdentity,TAggregateEvent>(TAggregateEvent @event, 
            Result[] commandResults = null, EventMetadata metadata = null)
            where TIdentity : Identity<TIdentity> where TAggregateEvent : IAggregateEvent<TIdentity> 
        {
            StartMonitoring();

            if (commandResults != null) _testKit.Platform.SetCommandResults(commandResults);
            var domainEvent = new DomainEvent<TIdentity, TAggregateEvent>(@event.Id, @event, DateTimeOffset.Now,
                1, metadata ?? EventMetadata.Empty);
            _saga.ProcessEvent(domainEvent).GetAwaiter().GetResult();
            _testKit.Platform.ClearCommandResults();
            
            StopMonitoring();
            return this;
        }

        public ISagaFixtureAsserter<TSaga, TSagaState> AndWhen<TIdentity, TAggregateEvent>(TAggregateEvent @event,
            Result[] commandResults = null,
            EventMetadata metadata = null) where TAggregateEvent : IAggregateEvent<TIdentity>
            where TIdentity : Identity<TIdentity>
            => When<TIdentity, TAggregateEvent>(@event, commandResults, metadata);

        public ISagaFixtureAsserter<TSaga, TSagaState> ThenExpect<TIdentity, TCommand>(Predicate<TCommand> commandPredicate = null) 
            where TIdentity : Identity<TIdentity> where TCommand : ICommand<TIdentity>
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


        public ISagaFixtureAsserter<TSaga, TSagaState> ThenExpectState(Predicate<TSagaState> aggregateEventPredicate)
        {
            Assert.True(aggregateEventPredicate?.Invoke(State),
                $"Невалидное состояние саги {typeof(TSaga).Name}");
            return this;
        }
    }
}