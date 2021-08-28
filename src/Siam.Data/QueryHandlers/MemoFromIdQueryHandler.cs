using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Platformex.Application;
using Siam.Application.Queries;
using Siam.Data.MemoContext;
using Siam.MemoContext;

namespace Siam.Data.QueryHandlers
{
    //Обработчик запроса
    public class MemoFromIdQueryHandler : QueryHandler<MemoFromIdQuery, MemoListItem>
    {
        private readonly MemoDbContext _context;

        public MemoFromIdQueryHandler(MemoDbContext context)
        {
            _context = context;
        }

        protected override Task<MemoListItem> ExecuteAsync(MemoFromIdQuery query)
        {
            var id = new MemoId(query.MemoId).GetGuid();
            return _context.Memos.Where(i => i.Id == id)
                .Select(i => new MemoListItem
                {
                    Id = MemoId.With(i.Id).Value,
                    Document = i.Model.Document,
                    History = i.Model.History,
                    Status = i.Model.Status
                }).FirstOrDefaultAsync();
        }
    }
}