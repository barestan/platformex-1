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
            //������� ��������� ��� �����
            var fixture = new AggregateFixture<MemoId, MemoAggregate, IMemoState, MemoState>(this);

            var id = MemoId.New;

            //��������� ���������
            var docId = Guid.NewGuid().ToString();
            var docNumber = new DocumentNumber("100");
            var docAddress = new Address("12700", "������", 
                "������", "�������� ����", "1");
            
            //BDD ���� (��������)
            fixture.For(id)
                
                //�������� (�����������)
                .GivenNothing()

                //����� (����������� ��������)
                .When(new UpdateMemo(id, 
                    new MemoDocument(docId, docNumber, docAddress)))

                //����� (�������� �����������)
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

            fixture.For()
                .GivenNothing()
                
                .When<MemoId, MemoUpdated>(new MemoUpdated(id, new MemoDocument(Guid.NewGuid().ToString(), 
                    new DocumentNumber("1"), new Address("127000", "������", "������", "�������� ����", "1"))))
                .ThenExpect<MemoId, SignMemo>(command => command.Id == id)
                
                .AndWhen<MemoId, MemoSigned>(new MemoSigned(id))
                .ThenExpect<MemoId, ConfirmSigningMemo>(e => e.Id == id);

        }
        
        
    }
}