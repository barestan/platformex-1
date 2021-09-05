using System;
using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IDbProvider<TModel>
    where TModel : IModel
    {
        /// <summary>
        /// Загрузить или создать модель, если ее нет в хранилище
        /// </summary>
        /// <param name="id"></param>
        /// <returns>(модель и флаг создания модели) </returns>
        Task<(TModel model, bool isCreated)> LoadOrCreate(Guid id);
        Task SaveChangesAsync(Guid id,  TModel model);
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}