using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;

namespace Platformex.Tests
{
    public abstract class Test
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        protected IFixture Fixture { get; private set; }
        protected ILogger Log => LogHelper.Logger;

        public  Test()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            Fixture.Customize<EventId>(c => c.FromFactory(() => EventId.New));
        }

        protected T A<T>()
        {
            return Fixture.Create<T>();
        }

        protected List<T> Many<T>(int count = 3)
        {
            return Fixture.CreateMany<T>(count).ToList();
        }

        protected T Mock<T>()
            where T : class
        {
            return new Mock<T>().Object;
        }

        protected static ILogger<T> Logger<T>()
        {
            return LogHelper.For<T>();
        }

        protected T Inject<T>(T instance)
            where T : class
        {
            Fixture.Inject(instance);
            return instance;
        }

        protected Mock<T> InjectMock<T>(params object[] args)
            where T : class
        {
            var mock = new Mock<T>(args);
            Fixture.Inject(mock.Object);
            return mock;
        }

        

        protected Mock<Func<T>> CreateFailingFunction<T>(T result, params Exception[] exceptions)
        {
            var function = new Mock<Func<T>>();
            var exceptionStack = new Stack<Exception>(exceptions.Reverse());
            function
                .Setup(f => f())
                .Returns(() =>
                {
                    if (exceptionStack.Any())
                    {
                        throw exceptionStack.Pop();
                    }
                    return result;
                });
            return function;
        }
    }
}
