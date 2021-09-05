using System;
using Xunit;
using Orleans.TestKit;
using Platformex.Application;
using Platformex.Tests;
using Siam.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;

namespace Siam.Tests
{
    public class UnitTest1 : PlatformexTestKit
    {
        public UnitTest1()
        {
            Silo.AddService<IMemoState>(new MemoState(new InMemoryDbProvider<MemoModel>()));
        }

        [Fact]
        public void UpdateMemoTest()
        {
            //Создаем заготовку для теста
            var fixture = new AggregateFixture<MemoId, MemoAggregate, IMemoState, MemoState>(this);

            var id = MemoId.New;

            //Параметры документа
            var docId = Guid.NewGuid().ToString();
            var docNumber = new DocumentNumber("100");
            var docAddress = new Address("12700", "Россия", 
                "Москва", "проспект Мира", "1");
            
            //BDD тест (сценарий)
            fixture.For(id)
                
                //Допустим (предусловия)
                .GivenNothing()

                //Когда (тестируемые действия)
                .When(new UpdateMemo(id, 
                    new MemoDocument(docId, docNumber, docAddress)))

                //Тогда (проверка результатов)
                .ThenExpectResult(e => e.IsSuccess)
                .ThenExpectDomainEvent<MemoUpdated>(e
                    => e.AggregateEvent.Id == id
                       && e.AggregateEvent.Document != null
                       && e.AggregateEvent.Document.Id == docId
                       && e.AggregateEvent.Document.Number == docNumber
                       && e.AggregateEvent.Document.CustomerAddress == docAddress)
                .ThenExpectState(s
                    => s.Status == MemoStatus.Undefined
                       && s.Document != null
                       && s.Document.Id == docId
                       && s.Document.Number == docNumber
                       && s.Document.CustomerAddress == docAddress);
        }
        [Fact]
        public void TestSaga()
        {
            var id = MemoId.New;
            var fixture = new SagaFixture<AutoConfimMemoSaga, AutoConfirmSagaState>(this);

            //BDD тест (сценарий)
            fixture.For()

                //Допустим (предусловия)
                .GivenNothing()

                //Когда (тестируемые действия)
                .When<MemoId, SigningStarted>(new SigningStarted(id, "TestUser"))

                //Тогда (проверка результатов)
                .ThenExpect<MemoId, ConfirmSigningMemo>(command => command.Id == id)

                //Когда (тестируемые действия)
                .AndWhen<MemoId, MemoSigned>(new MemoSigned(id))

                //Тогда (проверка результатов)
                .ThenExpectState(state => state.UserId == "TestUser");
        }

        [Fact]
        public void TestService()
        {
            var fixture = new ServiceFixture<IMemoService, MemoService>(this);

            //BDD тест (сценарий)
            fixture.For()

                //Допустим (предусловия)
                .GivenNothing()

                //Когда (тестируемые действия)
                .When(async service => await service.CreateMemos(2))

                //Тогда (проверка результатов)
                .ThenExpect<MemoId, UpdateMemo>(command => command.Document != null)
                .ThenExpect<MemoId, UpdateMemo>(command => command.Document != null);
        }
    }
}