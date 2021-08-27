using System;
using System.Collections.Generic;
using Xunit;
using Platformex.Application;
using Platformex.Domain;

namespace Platformex.Tests
{
    public class AggregateFixture<TIdentity, TAggregate, TState, TStateImpl> : IAggregateFixtureArranger<TAggregate, TIdentity,TState>,
        IAggregateFixtureExecutor<TAggregate, TIdentity, TState>, IAggregateFixtureAsserter<TAggregate, TIdentity,TState>
        where TAggregate : Aggregate<TIdentity, TState, TAggregate>
        where TIdentity : Identity<TIdentity>
        where TState :  IAggregateState<TIdentity>
        where TStateImpl : AggregateState<TIdentity,TStateImpl>, TState
    {
        private readonly PlatformexTestKit _testKit;
        private TAggregate _aggregate;
        private TState State => _aggregate.TestOnlyGetState(); 
        public TIdentity AggregateId => _aggregate?.GetId<TIdentity>();
        private readonly Stack<(Type,CommandResult)> _commandResults = new Stack<(Type,CommandResult)>();
        private readonly Stack<IDomainEvent> _events = new Stack<IDomainEvent>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public AggregateFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            _testKit.Builder.RegisterAggregate<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public IAggregateFixtureArranger<TAggregate, TIdentity, TState> For(TIdentity aggregateId)
        {
            _testKit.Platform.EventPublished += (_, args) =>
            {
                if (_isMonitoring) 
                    _events.Push(args.DomainEvent);
            };
            
            _aggregate = _testKit.TestKitSilo.CreateGrainAsync<TAggregate>(aggregateId.Value).GetAwaiter().GetResult();
            return this;
        }

        public IAggregateFixtureExecutor<TAggregate, TIdentity, TState> GivenNothing() => this;
        public IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(params IAggregateEvent<TIdentity>[] aggregateEvents)
        {
            foreach (var aggregateEvent in aggregateEvents)
            {
                State.Apply(aggregateEvent);
            }
            return this;
        }


        public IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(TState aggregateState)
        {
            _aggregate.TestOnlySetState(aggregateState);
            return this;
        }

        public IAggregateFixtureExecutor<TAggregate, TIdentity, TState> Given(params ICommand[] commands)
        {
            foreach (var command in commands)
            {
                _aggregate.DoAsync(command).GetAwaiter().GetResult();
            }

            return this;
        }


        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> When(params ICommand[] commands)
        {
            StartMonitoring();
            foreach (var command in commands)
            {
                _commandResults.Push((command.GetType(), _aggregate.DoAsync(command).GetAwaiter().GetResult()));
            }
            StopMonitoring();
            return this;
        }


        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> AndWhen(params ICommand[] commands) 
            => When(commands);

        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpect<TAggregateEvent>(
            Predicate<TAggregateEvent> aggregateEventPredicate = null) 
            where TAggregateEvent : class, IAggregateEvent<TIdentity>
        {
            if (_events.Count == 0)
                Assert.True(false, $"Нет ожидаемого события {typeof(TAggregateEvent).Name}.");
            var type = _events.Pop().GetType();
            Assert.True(type == typeof(TAggregateEvent),
                $"Невалидное событие, ожидалось {typeof(TAggregateEvent).Name} вместо {type.Name}");
            return this;
        }

        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectState(Predicate<TState> aggregateEventPredicate = null)
        {
            Assert.True(aggregateEventPredicate?.Invoke(State),
                $"Невалидное состояние агрегата {typeof(TAggregate).Name}");
            return this;
        }

        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectResult(Predicate<CommandResult> aggregateReply = null)
        {
            if (_commandResults.Count == 0)
                Assert.True(false, $"Нет ожидаемого результат команды.");

            var tuple = _commandResults.Pop(); 
            Assert.True(aggregateReply?.Invoke(tuple.Item2),
                    $"Невалидный результ выполнения команды {tuple.Item1.Name}");
            return this;
        }


        public IAggregateFixtureAsserter<TAggregate, TIdentity, TState> ThenExpectDomainEvent<TAggregateEvent>(
            Predicate<IDomainEvent<TIdentity, TAggregateEvent>> domainEventPredicate = null) 
            where TAggregateEvent : IAggregateEvent<TIdentity>
        {
            var @event = _events.Pop();
            Assert.True(@event.EventType == typeof(TAggregateEvent),
                $"Невалидное событие, ожидалось {typeof(TAggregateEvent).Name} вместо {@event.EventType.Name}");
            
            if (domainEventPredicate != null)
                Assert.True(domainEventPredicate.Invoke((IDomainEvent<TIdentity, TAggregateEvent>) @event),
                    $"Невалидное доменное событие {typeof(TAggregateEvent).Name}");
            return this;
        }
        
    }
}