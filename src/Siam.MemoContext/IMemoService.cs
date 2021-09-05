using System.ComponentModel;
using System.Threading.Tasks;
using Platformex;

namespace Siam.MemoContext
{

    public interface IMemoService : IService
    {
        [Description("Cозданиe заданного количества Памяток")]
        Task<int> CreateMemos(int memoCount);
    }
}