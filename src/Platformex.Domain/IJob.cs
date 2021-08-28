using System.Threading.Tasks;
using Orleans;

namespace Platformex.Domain
{
    public interface IJob : IGrainWithStringKey
    {
        Task ExecuteAsync();
    }
}
