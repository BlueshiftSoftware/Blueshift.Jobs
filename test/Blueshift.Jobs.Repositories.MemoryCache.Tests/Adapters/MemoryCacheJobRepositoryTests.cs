using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.Jobs.DomainModel;
using Blueshift.Jobs.DomainModel.SearchCriteria;
using Blueshift.Jobs.Repositories.MemoryCache.Adapters;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Blueshift.Jobs.Repositories.MemoryCache.Tests.Adapters
{
    public class MemoryCacheJobRepositoryTests
    {
        private TestMemoryCacheJobRepositoryFactory _testMemoryCacheJobRepositoryFactory;

        public MemoryCacheJobRepositoryTests()
        {
            _testMemoryCacheJobRepositoryFactory = new TestMemoryCacheJobRepositoryFactory();
        }

        [Fact]
        public async Task CreateJobAsync_assigns_JobId_and_saves_to_cache()
        {
            var job = new Job();

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.TryAddValue(It.IsAny<Guid>(), job))
                .Returns(true)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            Assert.Equal(Guid.Empty, job.JobId);

            Assert.Same(job, await memoryCacheJobRepository.CreateJobAsync(job));
            Assert.NotEqual(Guid.Empty, job.JobId);

            _testMemoryCacheJobRepositoryFactory.MockJobCache.Verify(
                jobCache => jobCache.TryAddValue(job.JobId, job),
                Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task CreateJobAsync_assigns_new_JobId_when_attempt_to_save_fails(int iterations)
        {
            var job = new Job();
            int count = 0;

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.TryAddValue(It.IsAny<Guid>(), job))
                .Returns((Guid guid, Job batch) => count++ < iterations ? false : true)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            Assert.Equal(Guid.Empty, job.JobId);

            Assert.Same(job, await memoryCacheJobRepository.CreateJobAsync(job));
            Assert.NotEqual(Guid.Empty, job.JobId);

            _testMemoryCacheJobRepositoryFactory.MockJobCache.Verify(
                jobCache => jobCache.TryAddValue(
                    It.Is<Guid>(guid => guid != job.JobId),
                    job),
                Times.Exactly(iterations));

            _testMemoryCacheJobRepositoryFactory.MockJobCache.Verify(
                jobCache => jobCache.TryAddValue(job.JobId, job),
                Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteJobAsync_removes_key_from_cache(bool removed)
        {
            Guid jobId = Guid.NewGuid();

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.TryRemoveValue(jobId))
                .Returns(removed)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            await memoryCacheJobRepository.DeleteJobAsync(jobId);

            _testMemoryCacheJobRepositoryFactory.MockJobCache.Verify(
                jobCache => jobCache.TryRemoveValue(jobId),
                Times.Once);
        }

        delegate void GetCachedJob(Guid batchId, out Job jobToGet);

        [Fact]
        public async Task GetJobAsync_return_Job_when_present_in_cache()
        {
            Job job = new Job
            {
                JobId = Guid.NewGuid()
            };

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.TryGetValue(
                    job.JobId,
                    out It.Ref<Job>.IsAny))
                .Callback(new GetCachedJob((Guid batchId, out Job jobToGet) =>
                {
                    jobToGet = job;
                }))
                .Returns(true)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            Assert.Same(job, await memoryCacheJobRepository.GetJobAsync(job.JobId));

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(jobCache => jobCache.TryGetValue(
                        job.JobId,
                        out It.Ref<Job>.IsAny),
                    Times.Once);
        }

        [Fact]
        public async Task GetJobAsync_returns_null_when_not_present_in_cache()
        {
            Guid missingKey = Guid.NewGuid();

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.TryGetValue(
                    missingKey,
                    out It.Ref<Job>.IsAny))
                .Callback(new GetCachedJob((Guid batchId, out Job jobToGet) =>
                {
                    jobToGet = default;
                }))
                .Returns(false)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            Assert.Null(await memoryCacheJobRepository.GetJobAsync(missingKey));

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(jobCache => jobCache.TryGetValue(
                        missingKey,
                        out It.Ref<Job>.IsAny),
                    Times.Once);
        }

        [Fact]
        public async Task UpdateJobAsync_sets_value_in_cache()
        {
            var job = new Job
            {
                JobId = Guid.NewGuid()
            };

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.SetValue(job.JobId, job))
                .Returns((Guid guid, Job batch) => batch)
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            Assert.Same(job, await memoryCacheJobRepository.UpdateJobAsync(job));

            _testMemoryCacheJobRepositoryFactory.MockJobCache.Verify(
                jobCache => jobCache.SetValue(job.JobId, job),
                Times.Once);
        }

        public static IEnumerable<object[]> JobOwners => TestMemoryCacheJobRepositoryFactory
            .OwnerIds
            .Select(ownerId => new[] { ownerId });

        [Theory]
        [MemberData(nameof(JobOwners))]
        public async Task GetJobsAsync_returns_set_of_jobs_by_owner(string ownerId)
        {
            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.Query())
                .Returns(() => TestMemoryCacheJobRepositoryFactory.Jobs.AsQueryable())
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            var jobSearchCriteria = new JobSearchCriteria
            {
                JobOwnerId = ownerId
            };

            IReadOnlyCollection<Job> actualJobs = await memoryCacheJobRepository.GetJobsAsync(jobSearchCriteria);
            IReadOnlyCollection<Job> expectedJobs = TestMemoryCacheJobRepositoryFactory.Jobs
                .Where(job => job.JobOwnerId.Contains(ownerId))
                .OrderBy(job => job.ExecuteAfter ?? job.CreatedAt)
                .ToList()
                .AsReadOnly();

            Assert.Equal(expectedJobs, actualJobs);

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(
                    jobCache => jobCache.Query(),
                    Times.Once);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(5, null)]
        [InlineData(null, 5)]
        [InlineData(5, 5)]
        public async Task GetJobsAsync_returns_constrained_results(int? skip, int? limit)
        {
            string ownerId = TestMemoryCacheJobRepositoryFactory.OwnerIds[0];

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.Query())
                .Returns(() => TestMemoryCacheJobRepositoryFactory.Jobs.AsQueryable())
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            var jobSearchCriteria = new JobSearchCriteria
            {
                JobOwnerId = ownerId,
                ItemsToSkip = skip,
                MaximumItems = limit
            };

            IReadOnlyCollection<Job> actualJobs = await memoryCacheJobRepository.GetJobsAsync(jobSearchCriteria);
            IEnumerable<Job> expectedJobs = TestMemoryCacheJobRepositoryFactory.Jobs
                .Where(job => job.JobOwnerId.Contains(ownerId))
                .OrderBy(job => job.ExecuteAfter ?? job.CreatedAt);

            if (skip != null)
            {
                expectedJobs = expectedJobs.Skip(skip.Value);
            }

            if (limit != null)
            {
                expectedJobs = expectedJobs.Take(limit.Value);
            }

            Assert.Equal(expectedJobs, actualJobs);

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(
                    jobCache => jobCache.Query(),
                    Times.Once);
        }

        [Theory]
        [InlineData(JobStatus.Created)]
        [InlineData(JobStatus.Pending)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.Cancelled)]
        [InlineData(JobStatus.Failed)]
        [InlineData(JobStatus.Pending, JobStatus.Completed)]
        [InlineData(JobStatus.Created, JobStatus.Cancelled, JobStatus.Failed)]
        public async Task GetJobsAsync_returns_Jobs_by_JobStatus(params JobStatus[] jobStatuses)
        {
            string ownerId = TestMemoryCacheJobRepositoryFactory.OwnerIds[0];

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.Query())
                .Returns(() => TestMemoryCacheJobRepositoryFactory.Jobs.AsQueryable())
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            var jobSearchCriteria = new JobSearchCriteria
                {
                    JobOwnerId = ownerId
                }
                .WithJobStatuses(jobStatuses);

            IReadOnlyCollection<Job> actualJobs = await memoryCacheJobRepository.GetJobsAsync(jobSearchCriteria);
            IReadOnlyCollection<Job> expectedJobs = TestMemoryCacheJobRepositoryFactory.Jobs
                .Where(job => job.JobOwnerId.Contains(ownerId) && jobStatuses.Contains(job.JobStatus))
                .OrderBy(job => job.ExecuteAfter ?? job.CreatedAt)
                .ToList()
                .AsReadOnly();

            Assert.Equal(expectedJobs, actualJobs);

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(
                    jobCache => jobCache.Query(),
                    Times.Once);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(30)]
        public async Task GetJobsAsync_returns_Jobs_on_or_after_ExecuteAfter_marker(int minutesAfterMarker)
        {
            string ownerId = TestMemoryCacheJobRepositoryFactory.OwnerIds[0];

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Setup(jobCache => jobCache.Query())
                .Returns(() => TestMemoryCacheJobRepositoryFactory.Jobs.AsQueryable())
                .Verifiable();

            var memoryCacheJobRepository = _testMemoryCacheJobRepositoryFactory.CreateMemoryCacheJobRepository();

            DateTimeOffset executeAfter = TestMemoryCacheJobRepositoryFactory.ExecuteAfterMarker.AddMinutes(minutesAfterMarker);

            var jobSearchCriteria = new JobSearchCriteria
            {
                JobOwnerId = ownerId,
                ExecuteAfter = executeAfter
            };

            IReadOnlyCollection<Job> actualJobs = await memoryCacheJobRepository.GetJobsAsync(jobSearchCriteria);
            IReadOnlyCollection<Job> expectedJobs = TestMemoryCacheJobRepositoryFactory.Jobs
                .Where(job => job.JobOwnerId.Contains(ownerId) && executeAfter >= job.ExecuteAfter)
                .OrderBy(job => job.ExecuteAfter ?? job.CreatedAt)
                .ToList()
                .AsReadOnly();

            Assert.Equal(expectedJobs, actualJobs);

            _testMemoryCacheJobRepositoryFactory.MockJobCache
                .Verify(
                    jobCache => jobCache.Query(),
                    Times.Once);
        }

        private class TestMemoryCacheJobRepositoryFactory
        {
            private static readonly int _jobStatusCount = Enum.GetValues(typeof(JobStatus)).Length;
            private static readonly Random _random = new Random();

            public static readonly DateTimeOffset ExecuteAfterMarker = DateTimeOffset.UtcNow.AddMinutes(30);

            public static ReadOnlyCollection<string> OwnerIds { get; } = Enumerable.Range(1, 5)
                .Select(number => $"owner-id-{number}")
                .ToList()
                .AsReadOnly();

            public static ReadOnlyCollection<Job> Jobs { get; } = Enumerable.Range(1, 10 * OwnerIds.Count * _jobStatusCount)
                .Select(number => new
                {
                    JobNumber = number,
                    OwnerIndex = number % OwnerIds.Count,
                    JobStatusOffset = (number / OwnerIds.Count) % _jobStatusCount,
                    Immediate = ((number / OwnerIds.Count / _jobStatusCount) % 2) == 0
                })
                .Select(tuple => new Job
                {
                    JobId = Guid.NewGuid(),
                    JobDescription = $"Job #{tuple.JobNumber}",
                    JobStatus = (JobStatus)(JobStatus.Created + tuple.JobStatusOffset),
                    JobOwnerId = OwnerIds[tuple.OwnerIndex],
                    CreatedAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(_random.NextDouble() * 60 * 24)),
                    ExecuteAfter = tuple.Immediate
                        ? default
                        : ExecuteAfterMarker
                })
                .ToList()
                .AsReadOnly();

            public Mock<ICache<Guid, Job>> MockJobCache { get; } = new Mock<ICache<Guid, Job>>();

            public MemoryCacheJobRepository CreateMemoryCacheJobRepository()
                => new MemoryCacheJobRepository(
                    NullLogger<MemoryCacheJobRepository>.Instance,
                    MockJobCache.Object);
        }
    }
}
