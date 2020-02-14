using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.Requests;

namespace Blueshift.Jobs.Abstractions.Services
{
    public interface IJobBatchService
    {
        Task<JobBatch> CreateJobBatchAsync(CreateJobBatchRequest createJobBatchRequest);
    }
}
