using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface IAggregateState<TIdentity> where TIdentity : Identity<TIdentity>
    {
        TIdentity Identity {get;}
       
        /// <summary>
        /// Загрузка состояния
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>true - если состояние создано (новый объект) false - состояние загружено из БД</returns>
        Task<bool> LoadState(TIdentity id);
        Task Apply(IAggregateEvent<TIdentity> e);

        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}