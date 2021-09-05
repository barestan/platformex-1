using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex.Domain;
using Xunit;

namespace Platformex.Tests
{
    public class ServiceFixture<TServiceInterface, TService> : IServiceFixtureArranger<TServiceInterface,TService>,
        IServiceFixtureExecutor<TServiceInterface,TService>, IServiceFixtureAsserter<TServiceInterface,TService>
        where TService : ServiceBase, TServiceInterface
        where TServiceInterface : IService
    {
        private readonly PlatformexTestKit _testKit;
        private TService _service;
        // ReSharper disable once UnusedMember.Local
        private readonly Stack<ICommand> _commands = new Stack<ICommand>();
        
        private bool _isMonitoring;
        private void StopMonitoring() => _isMonitoring = false;
        private void StartMonitoring()=> _isMonitoring = true;


        public ServiceFixture(PlatformexTestKit testKit)
        {
            _testKit = testKit;
            //_testKit.Builder.Register<TIdentity, TAggregate, TStateImpl>().WithCommands();
        }
        public IServiceFixtureArranger<TServiceInterface,TService> For()
        {
            _testKit.Platform.CommandExecuted += (_, args) =>
            {
                if (_isMonitoring) 
                    _commands.Push(args.Command);
            };
            
            _service = _testKit.TestKitSilo.CreateGrainAsync<TService>(Guid.NewGuid()).GetAwaiter().GetResult();
            return this;
        }

        public IServiceFixtureExecutor<TServiceInterface,TService> GivenNothing() => this;
        private ServiceMetadata _metadata = new ServiceMetadata();
        public IServiceFixtureExecutor<TServiceInterface, TService> GivenMetadata(ServiceMetadata metadata)
        {
            _metadata = metadata;
            return this;
        }

        public IServiceFixtureAsserter<TServiceInterface,TService> AndWhen(Func<TServiceInterface, Task<object>> testFunc) 
            => When(testFunc);

        public IServiceFixtureAsserter<TServiceInterface,TService> ThenExpect<TIdentity, TCommand>(Predicate<TCommand> commandPredicate = null) 
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

        public IServiceFixtureAsserter<TServiceInterface,TService> ThenExpectResult<TResult>(Predicate<TResult> resultPredicate = null)
        {
            if (_results.Count == 0)
                Assert.True(false, $"Нет ожидаемого результата.");

            var tuple = _results.Pop(); 

            Assert.True(tuple.Item1 != typeof(TResult),
                $"Неверный тип результата, ожидался{typeof(TResult)} вместо {tuple.Item1}");

            Assert.True(resultPredicate?.Invoke((TResult)tuple.Item2),
                $"Невалидный результат выполнения сервиса");
            return this;
        }

        private readonly Stack<(Type,object)> _results = new();
        public IServiceFixtureAsserter<TServiceInterface,TService> When(Func<TServiceInterface, Task<object>> testFunc)
        {
            StartMonitoring();
            _service.SetMetadata(_metadata);
            var result = testFunc(_service).GetAwaiter().GetResult();
            if (result == null)
            {
                _results.Push((typeof(object), null));
                return this;
            }
            _results.Push((result.GetType(), result));
            StopMonitoring();
            return this;
        }
    }
}