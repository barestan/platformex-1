using System.ComponentModel;
using System.Threading.Tasks;
using Platformex;

namespace Siam.MemoContext
{
    [Description("Cозданиe заданного количества Памяток")]

    public interface IMemoService : IDomainService
    {
        //Тестовый сервис по созданию заданного количества Памяток
        Task<Result> CreateMemos(int memoCount);
    }
}