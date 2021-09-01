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
            var rnd = new Random();
            await ExecuteAsync(new UpdateMemo(MemoId.New, 
                new MemoDocument(
                    Guid.NewGuid().ToString(),
                    new DocumentNumber(rnd.Next(1000).ToString()),
                    new Address("127000", "Россия", "Москва",
                        "проспект Мира",rnd.Next(1000).ToString()
                    ))));
        }

        protected override async Task Initialize()
        {
            await RegisterOrUpdateJob(TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
    }
}