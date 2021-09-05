using System.Threading.Tasks;
using Orleans;

namespace Platformex
{
    public interface IService : IGrainWithGuidKey
    {
        Task SetMetadata(ServiceMetadata metadata);
    }
}