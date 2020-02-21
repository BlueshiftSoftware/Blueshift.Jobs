using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.Requests;
using Blueshift.Jobs.DomainModel.SearchCriteria;
using Blueshift.Jobs.Ports.Repositories;
using Blueshift.Jobs.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Blueshift.Jobs.Tests.Services
{
    public class JobBatchServiceTests
    {
        private readonly TestJobBatchServiceFactory _testJobBatchServiceFactory;

        public JobBatchServiceTests()
        {
            _testJobBatchServiceFactory = new TestJobBatchServiceFactory();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(150)]
        public async Task CreateJobBatchAsync_requests_the_expected_number_of_jobs_and_saves_the_batch(int requestedJobCount)
        {
            _testJobBatchServiceFactory.MockJobRepository
                .Setup(jobRepository => jobRepository.GetJobsAsync(It.IsAny<JobSearchCriteria>()))
                .ReturnsAsync((JobSearchCriteria jobSearchCriteria) =>
                    _testJobBatchServiceFactory.AvailableJobs
                        .Take(jobSearchCriteria.MaximumItems.Value)
                        .ToList()
                        .AsReadOnly()
                )
                .Verifiable();

            _testJobBatchServiceFactory.MockJobBatchRepository
                .Setup(jobBatchRepository => jobBatchRepository.CreateJobBatchAsync(It.IsAny<JobBatch>()))
                .ReturnsAsync((JobBatch batch) => batch)
                .Verifiable();

            JobBatchService jobBatchService = _testJobBatchServiceFactory.CreateJobBatchService();

            var createJobBatchRequest = new CreateJobBatchRequest()
            {
                BatchingJobFilter =
                {
                    MaximumItems = requestedJobCount
                }
            };

            JobBatch jobBatch = await jobBatchService.CreateJobBatchAsync(createJobBatchRequest);

            Assert.NotNull(jobBatch);

            Assert.Equal(
                _testJobBatchServiceFactory.AvailableJobs.Take(requestedJobCount),
                jobBatch.Jobs);

            _testJobBatchServiceFactory.MockJobRepository
                .Verify(
                    jobRepository => jobRepository.GetJobsAsync(
                        It.Is<JobSearchCriteria>(jobSearchCriteria => jobSearchCriteria.MaximumItems == requestedJobCount)),
                    Times.Once);

            _testJobBatchServiceFactory.MockJobBatchRepository
                .Verify(
                    jobBatchRepository => jobBatchRepository.CreateJobBatchAsync(jobBatch),
                    Times.Once);
        }

        private class TestJobBatchServiceFactory
        {
            public IList<Job> AvailableJobs { get; } = Enumerable
                .Range(0, 100)
                .Select(index => new Job
                {
                    JobId = Guid.NewGuid(),
                    JobDescription = $"Job Number {index}"
                })
                .ToList();

            public Mock<IJobRepository> MockJobRepository { get; } = new Mock<IJobRepository>();

            public Mock<IJobBatchRepository> MockJobBatchRepository { get; } = new Mock<IJobBatchRepository>();

            public JobBatchService CreateJobBatchService()
                => new JobBatchService(
                    MockJobRepository.Object,
                    MockJobBatchRepository.Object,
                    NullLogger<JobBatchService>.Instance);
        }
    }
}
