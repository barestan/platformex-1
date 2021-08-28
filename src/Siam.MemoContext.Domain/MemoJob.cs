using System;
using System.Threading.Tasks;
using Platformex.Domain;

namespace Siam.MemoContext.Domain
{
    [Subscriber]
    public class MemoJob : Job
    {
        public override async Task ExecuteAsync()
        {
            await ExecuteAsync(new SignMemo(MemoId.New, String.Empty));
        }

        protected override async Task Initialize()
        {
            await RegisterOrUpdateJob(TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
    }
}