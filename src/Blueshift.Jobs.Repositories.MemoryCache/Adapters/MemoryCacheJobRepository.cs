using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.SearchCriteria;
using Blueshift.Jobs.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace Blueshift.Jobs.Repositories.MemoryCache.Adapters
{
    public class MemoryCacheJobRepository : IJobRepository
    {
        private readonly ILogger<MemoryCacheJobRepository> _logger;
        private ICache<Guid, Job> _jobCache;

        public MemoryCacheJobRepository(
            ILogger<MemoryCacheJobRepository> logger,
            ICache<Guid, Job> jobCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobCache = jobCache ?? throw new ArgumentNullException(nameof(jobCache));
        }

        public Task<Job> CreateJobAsync(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            Guid jobId;

            do
            {
                jobId = Guid.NewGuid();
            }
            while (!_jobCache.TryAddValue(jobId, job));

            job.JobId = jobId;

            return Task.FromResult(job);
        }

        public Task DeleteJobAsync(Guid jobId)
        {
            _jobCache.TryRemoveValue(jobId);

            return Task.CompletedTask;
        }

        public Task<Job> GetJobAsync(Guid jobId)
            => _jobCache.TryGetValue(jobId, out Job job)
                ? Task.FromResult(job)
                : Task.FromResult<Job>(null);

        public Task<IReadOnlyCollection<Job>> GetJobsAsync(JobSearchCriteria jobSearchCriteria)
        {
            if (jobSearchCriteria == null)
            {
                throw new ArgumentNullException(nameof(jobSearchCriteria));
            }

            IQueryable<Job> jobQuery = _jobCache.Query();

            if (!string.IsNullOrEmpty(jobSearchCriteria.JobOwnerId))
            {
                jobQuery = jobQuery.Where(job => job.JobOwnerId.Contains(jobSearchCriteria.JobOwnerId, StringComparison.OrdinalIgnoreCase));
            }

            if (jobSearchCriteria.JobStatuses.Count > 0)
            {
                jobQuery = jobQuery.Where(job => jobSearchCriteria.JobStatuses.Contains(job.JobStatus));
            }

            if (jobSearchCriteria.ExecuteAfter.HasValue)
            {
                jobQuery = jobQuery.Where(job => jobSearchCriteria.ExecuteAfter >= job.ExecuteAfter);
            }

            jobQuery = jobQuery.OrderBy(job => job.ExecuteAfter ?? job.CreatedAt);

            if (jobSearchCriteria.ItemsToSkip.HasValue)
            {
                jobQuery = jobQuery.Skip(jobSearchCriteria.ItemsToSkip.Value);
            }

            if (jobSearchCriteria.MaximumItems.HasValue)
            {
                jobQuery = jobQuery.Take(jobSearchCriteria.MaximumItems.Value);
            }

            IReadOnlyCollection<Job> jobes = jobQuery
                .ToList()
                .AsReadOnly();

            return Task.FromResult(jobes);
        }

        public Task<Job> UpdateJobAsync(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            job = _jobCache.SetValue(
                job.JobId,
                job);

            return Task.FromResult(job);
        }
    }
}
