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
    public class MemoryCacheJobBatchRepository : IJobBatchRepository
    {
        private readonly ILogger<MemoryCacheJobBatchRepository> _logger;
        private ICache<Guid, JobBatch> _jobBatchCache;

        public MemoryCacheJobBatchRepository(
            ILogger<MemoryCacheJobBatchRepository> logger,
            ICache<Guid, JobBatch> jobBatchCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobBatchCache = jobBatchCache ?? throw new ArgumentNullException(nameof(jobBatchCache));
        }

        public Task<JobBatch> CreateJobBatchAsync(JobBatch jobBatch)
        {
            if (jobBatch == null)
            {
                throw new ArgumentNullException(nameof(jobBatch));
            }

            Guid jobBatchId;

            do
            {
                jobBatchId = Guid.NewGuid();
            }
            while (!_jobBatchCache.TryAddValue(jobBatchId, jobBatch));

            jobBatch.JobBatchId = jobBatchId;

            return Task.FromResult(jobBatch);
        }

        public Task DeleteJobBatchAsync(Guid jobBatchId)
        {
            _jobBatchCache.TryRemoveValue(jobBatchId);

            return Task.CompletedTask;
        }

        public Task<JobBatch> GetJobBatchAsync(Guid jobBatchId)
            => _jobBatchCache.TryGetValue(jobBatchId, out JobBatch jobBatch)
                ? Task.FromResult(jobBatch)
                : Task.FromResult<JobBatch>(null);

        public Task<IReadOnlyCollection<JobBatch>> GetJobBatchesAsync(JobBatchSearchCriteria jobBatchSearchCriteria)
        {
            if (jobBatchSearchCriteria == null)
            {
                throw new ArgumentNullException(nameof(jobBatchSearchCriteria));
            }

            IQueryable<JobBatch> jobBatchQuery = _jobBatchCache.Query();

            if (!string.IsNullOrEmpty(jobBatchSearchCriteria.JobBatchOwnerId))
            {
                jobBatchQuery = jobBatchQuery.Where(jobBatch => jobBatch.JobBatchOwnerId.Contains(jobBatchSearchCriteria.JobBatchOwnerId, StringComparison.OrdinalIgnoreCase));
            }

            jobBatchQuery = jobBatchQuery.OrderByDescending(jobBatch => jobBatch.CreatedAt);

            if (jobBatchSearchCriteria.ItemsToSkip.HasValue)
            {
                jobBatchQuery = jobBatchQuery.Skip(jobBatchSearchCriteria.ItemsToSkip.Value);
            }

            if (jobBatchSearchCriteria.MaximumItems.HasValue)
            {
                jobBatchQuery = jobBatchQuery.Take(jobBatchSearchCriteria.MaximumItems.Value);
            }

            IReadOnlyCollection<JobBatch> jobBatches = jobBatchQuery
                .ToList()
                .AsReadOnly();

            return Task.FromResult(jobBatches);
        }

        public Task<JobBatch> UpdateJobBatchAsync(JobBatch jobBatch)
        {
            if (jobBatch == null)
            {
                throw new ArgumentNullException(nameof(jobBatch));
            }

            jobBatch = _jobBatchCache.SetValue(
                jobBatch.JobBatchId,
                jobBatch);

            return Task.FromResult(jobBatch);
        }
    }
}
