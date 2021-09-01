using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface ISagaState 
    {
        /// <summary>
        /// Загрузка состояния
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>true - если состояние создано (новый объект) false - состояние загружено из БД</returns>
        Task<bool> LoadState(string id);
        Task SaveState(string id);
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}