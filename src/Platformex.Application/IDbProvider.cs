using System;
using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IDbProvider<TModel>
    {
        Task<(TModel model, bool isCreated)> LoadOrCreate(Guid id);
        Task SaveChangesAsync(Guid id,  TModel model);
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
    }
}