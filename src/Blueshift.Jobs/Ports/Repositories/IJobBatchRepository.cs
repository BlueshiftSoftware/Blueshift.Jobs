using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.SearchCriteria;

namespace Blueshift.Jobs.Ports.Repositories
{
    public interface IJobBatchRepository
    {
        Task<IReadOnlyCollection<JobBatch>> GetJobBatchesAsync(JobBatchSearchCriteria jobBatchSearchCriteria);

        Task<JobBatch> CreateJobBatchAsync(JobBatch jobBatch);

        Task DeleteJobBatchAsync(Guid jobBatchId);

        Task<JobBatch> GetJobBatchAsync(Guid jobBatchId);

        Task<JobBatch> UpdateJobBatchAsync(JobBatch jobBatch);
    }
}
