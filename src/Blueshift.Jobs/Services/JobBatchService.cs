using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.Jobs.Abstractions.Services;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.Requests;
using Blueshift.Jobs.Ports.Repositories;
using Blueshift.Jobs.Properties;
using Microsoft.Extensions.Logging;

namespace Blueshift.Jobs.Services
{
    public class JobBatchService : IJobBatchService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobBatchRepository _jobBatchRepository;
        private readonly ILogger<JobBatchService> _jobBatchServiceLogger;

        public JobBatchService(
            IJobRepository jobRepository,
            IJobBatchRepository jobBatchRepository,
            ILogger<JobBatchService> jobBatchServiceLogger)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _jobBatchRepository = jobBatchRepository ?? throw new ArgumentNullException(nameof(jobBatchRepository));
            _jobBatchServiceLogger = jobBatchServiceLogger ?? throw new ArgumentNullException(nameof(jobBatchServiceLogger));            
        }

        public async Task<JobBatch> CreateJobBatchAsync(CreateJobBatchRequest createJobBatchRequest)
        {
            if (createJobBatchRequest == null)
            {
                throw new ArgumentNullException(nameof(createJobBatchRequest));
            }

            _jobBatchServiceLogger.LogInformation(
                BlueshiftJobsResources.CreatingJobBatch,
                createJobBatchRequest.JobBatchRequestor,
                createJobBatchRequest.JobBatchDescription,
                createJobBatchRequest.BatchingJobFilter.MaximumJobCount);

            try
            {
                IReadOnlyCollection<Job> jobs = await _jobRepository
                    .GetJobsAsync(createJobBatchRequest.BatchingJobFilter)
                    .ConfigureAwait(false);

                var jobBatch = new JobBatch(jobs)
                {
                    JobBatchOwner = createJobBatchRequest.JobBatchRequestor,
                    JobBatchDescription = createJobBatchRequest.JobBatchDescription
                };

                jobBatch = await _jobBatchRepository
                    .CreateJobBatchAsync(jobBatch)
                    .ConfigureAwait(false);

                _jobBatchServiceLogger.LogInformation(
                    BlueshiftJobsResources.CreatedJobBatch,
                    jobBatch.JobBatchOwner,
                    jobBatch.JobBatchDescription,
                    jobBatch.Jobs.Count);

                return jobBatch;
            }
            catch (Exception e)
            {
                _jobBatchServiceLogger.LogError(
                    BlueshiftJobsResources.ErrorCreatingJobBatch,
                    _jobBatchServiceLogger.IsEnabled(LogLevel.Debug)
                        ? e.StackTrace
                        : e.Message);

                throw;
            }
        }
    }
}
