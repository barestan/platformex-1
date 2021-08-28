using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Platformex.Application;
using Siam.Application;

namespace Siam.Data.MemoContext
{

    public class MemoDbProvider : IDbProvider<MemoModel>
    {
        private readonly MemoDbContext _dbContext;

        public MemoDbProvider(MemoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private async Task<MemoModel> FindAsync(Guid id) 
            => await _dbContext.Memos.Where(i => i.Id == id).Select(i=>i.Model).FirstOrDefaultAsync();

        private MemoModel Create(Guid id) => new MemoModel {Id = id};

        public async Task<(MemoModel model, bool isCreated)> LoadOrCreate(Guid id)
        {
            var model = await FindAsync(id);
            var isCreated = true;
            if (model == null)
            {
                isCreated = false;
                model = Create(id);
            }

            return (model, isCreated);
        }
        public async Task SaveChangesAsync(Guid id, MemoModel model)
        {
            var item = new Memo { Id = model.Id, Model = model};
            if (await _dbContext.Memos.AnyAsync(i => i.Id == model.Id))
                _dbContext.Update(item);
            else
                _dbContext.Add(item);
            await _dbContext.SaveChangesAsync();
            
            _dbContext.Entry(item).State = EntityState.Detached;
        }

        public async Task BeginTransaction()
        {
            await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }
    }
}
