using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platformex;
using Platformex.Domain;
using Siam.MemoContext;

namespace Siam.Application
{
    public class AutoConfirmSagaState : ISagaState
    {
        public string UserId { get; set; }

        public Task<bool> LoadState(string id) => Task.FromResult(true);

        public Task SaveState(string id) => Task.CompletedTask;

        public Task BeginTransaction() => Task.CompletedTask;

        public Task CommitTransaction() => Task.CompletedTask;

        public Task RollbackTransaction() => Task.CompletedTask;
    }

    [Subscriber]
    public class AutoConfimMemoSaga : Saga<AutoConfirmSagaState,AutoConfimMemoSaga>,
        IStartedBy<MemoId,SigningStarted>,
        ISubscribeTo<MemoId,MemoSigned>
    {
        public async Task<string> HandleAsync(IDomainEvent<MemoId, SigningStarted> domainEvent)
        {

            //Сохраняем информацию в состоянии саги
            State.UserId = domainEvent.AggregateEvent.UserId;
            await ExecuteAsync(new ConfirmSigningMemo(domainEvent.AggregateIdentity));
            return domainEvent.AggregateEvent.Id.Value;
        }

        public Task HandleAsync(IDomainEvent<MemoId, MemoSigned> domainEvent)
        {
            //Можем получить доступ к ранее сохраненной информации в состоянии
            Logger.LogInformation($"Пользователь {State.UserId} успешно завершил подписание документа");
            return Task.CompletedTask;
        }
    }
}