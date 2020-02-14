using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.SearchCriteria;

namespace Blueshift.Jobs.Ports.Repositories
{
    public interface IJobRepository
    {
        Task<IReadOnlyCollection<Job>> GetJobsAsync(JobSearchCriteria jobSearchCriteria);

        Task<Job> CreateJobAsync(Job job);

        Task DeleteJobAsync(Guid jobId);

        Task<Job> GetJobAsync(Guid jobId);

        Task<Job> UpdateJobAsync(Job job);
    }
}
