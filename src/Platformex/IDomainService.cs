using System.Threading.Tasks;
using Orleans;

namespace Platformex
{
    public interface IDomainService : IGrainWithGuidKey
    {
        Task SetMetadata(ServiceMetadata metadata);
    }
}