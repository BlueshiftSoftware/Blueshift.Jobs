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
    public class MemoryCacheJobBatchRepositoryTests
    {
        private TestMemoryCacheJobBatchRepositoryFactory _testMemoryCacheJobBatchRepositoryFactory;

        public MemoryCacheJobBatchRepositoryTests()
        {
            _testMemoryCacheJobBatchRepositoryFactory = new TestMemoryCacheJobBatchRepositoryFactory();
        }

        [Fact]
        public async Task CreateJobBatchAsync_assigns_JobBatchId_and_saves_to_cache()
        {
            var jobBatch = new JobBatch();

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.TryAddValue(It.IsAny<Guid>(), jobBatch))
                .Returns(true)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            Assert.Equal(Guid.Empty, jobBatch.JobBatchId);

            Assert.Same(jobBatch, await memoryCacheJobBatchRepository.CreateJobBatchAsync(jobBatch));
            Assert.NotEqual(Guid.Empty, jobBatch.JobBatchId);

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache.Verify(
                jobBatchCache => jobBatchCache.TryAddValue(jobBatch.JobBatchId, jobBatch),
                Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task CreateJobBatchAsync_assigns_new_JobBatchId_when_attempt_to_save_fails(int iterations)
        {
            var jobBatch = new JobBatch();
            int count = 0;

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.TryAddValue(It.IsAny<Guid>(), jobBatch))
                .Returns((Guid guid, JobBatch batch) => count++ < iterations ? false : true)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            Assert.Equal(Guid.Empty, jobBatch.JobBatchId);

            Assert.Same(jobBatch, await memoryCacheJobBatchRepository.CreateJobBatchAsync(jobBatch));
            Assert.NotEqual(Guid.Empty, jobBatch.JobBatchId);

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache.Verify(
                jobBatchCache => jobBatchCache.TryAddValue(
                    It.Is<Guid>(guid => guid != jobBatch.JobBatchId),
                    jobBatch),
                Times.Exactly(iterations));

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache.Verify(
                jobBatchCache => jobBatchCache.TryAddValue(jobBatch.JobBatchId, jobBatch),
                Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteJobBatchAsync_removes_key_from_cache(bool removed)
        {
            Guid jobBatchId = Guid.NewGuid();

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.TryRemoveValue(jobBatchId))
                .Returns(removed)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            await memoryCacheJobBatchRepository.DeleteJobBatchAsync(jobBatchId);

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache.Verify(
                jobBatchCache => jobBatchCache.TryRemoveValue(jobBatchId),
                Times.Once);
        }

        delegate void GetCachedJobBatch(Guid batchId, out JobBatch jobBatchToGet);

        [Fact]
        public async Task GetJobBatchAsync_return_JobBatch_when_present_in_cache()
        {
            JobBatch jobBatch = new JobBatch
            {
                JobBatchId = Guid.NewGuid()
            };

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.TryGetValue(
                    jobBatch.JobBatchId,
                    out It.Ref<JobBatch>.IsAny))
                .Callback(new GetCachedJobBatch((Guid batchId, out JobBatch jobBatchToGet) =>
                {
                    jobBatchToGet = jobBatch;
                }))
                .Returns(true)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            Assert.Same(jobBatch, await memoryCacheJobBatchRepository.GetJobBatchAsync(jobBatch.JobBatchId));

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Verify(
                    jobBatchCache => jobBatchCache.TryGetValue(
                        jobBatch.JobBatchId,
                        out It.Ref<JobBatch>.IsAny),
                    Times.Once);
        }

        [Fact]
        public async Task GetJobBatchAsync_returns_null_when_not_present_in_cache()
        {
            Guid missingKey = Guid.NewGuid();

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.TryGetValue(
                    missingKey,
                    out It.Ref<JobBatch>.IsAny))
                .Callback(new GetCachedJobBatch((Guid batchId, out JobBatch jobBatchToGet) =>
                {
                    jobBatchToGet = default;
                }))
                .Returns(false)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            Assert.Null(await memoryCacheJobBatchRepository.GetJobBatchAsync(missingKey));

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Verify(
                    jobBatchCache => jobBatchCache.TryGetValue(
                        missingKey,
                        out It.Ref<JobBatch>.IsAny),
                    Times.Once);
        }

        [Fact]
        public async Task UpdateJobBatchAsync_sets_value_in_cache()
        {
            var jobBatch = new JobBatch
            {
                JobBatchId = Guid.NewGuid()
            };

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.SetValue(jobBatch.JobBatchId, jobBatch))
                .Returns((Guid guid, JobBatch batch) => batch)
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            Assert.Same(jobBatch, await memoryCacheJobBatchRepository.UpdateJobBatchAsync(jobBatch));

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache.Verify(
                jobBatchCache => jobBatchCache.SetValue(jobBatch.JobBatchId, jobBatch),
                Times.Once);
        }

        public static IEnumerable<object[]> JobBatchOwners => TestMemoryCacheJobBatchRepositoryFactory
            .OwnerIds
            .Select(ownerId => new[] { ownerId });

        [Theory]
        [MemberData(nameof(JobBatchOwners))]
        public async Task GetJobBatchesAsync_returns_set_of_jobs_by_owner(string ownerId)
        {
            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.Query())
                .Returns(() => TestMemoryCacheJobBatchRepositoryFactory.JobBatches.AsQueryable())
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            var jobSearchCriteria = new JobBatchSearchCriteria
            {
                JobBatchOwnerId = ownerId
            };

            IReadOnlyCollection<JobBatch> actualJobBatches = await memoryCacheJobBatchRepository.GetJobBatchesAsync(jobSearchCriteria);
            IReadOnlyCollection<JobBatch> expectedJobBatches = TestMemoryCacheJobBatchRepositoryFactory.JobBatches
                .Where(jobBatch => jobBatch.JobBatchOwnerId.Contains(ownerId))
                .OrderByDescending(jobBatch => jobBatch.CreatedAt)
                .ToList()
                .AsReadOnly();

            Assert.Equal(expectedJobBatches, actualJobBatches);

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Verify(
                    jobBatchCache => jobBatchCache.Query(),
                    Times.Once);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(5, null)]
        [InlineData(null, 5)]
        [InlineData(5, 5)]
        public async Task GetJobBatchesAsync_returns_constrained_results(int? skip, int? limit)
        {
            string ownerId = TestMemoryCacheJobBatchRepositoryFactory.OwnerIds[0];

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Setup(jobBatchCache => jobBatchCache.Query())
                .Returns(() => TestMemoryCacheJobBatchRepositoryFactory.JobBatches.AsQueryable())
                .Verifiable();

            var memoryCacheJobBatchRepository = _testMemoryCacheJobBatchRepositoryFactory.CreateMemoryCacheJobBatchRepository();

            var jobSearchCriteria = new JobBatchSearchCriteria
            {
                JobBatchOwnerId = ownerId,
                ItemsToSkip = skip,
                MaximumItems = limit
            };

            IReadOnlyCollection<JobBatch> actualJobBatches = await memoryCacheJobBatchRepository.GetJobBatchesAsync(jobSearchCriteria);
            IEnumerable<JobBatch> expectedJobBatches = TestMemoryCacheJobBatchRepositoryFactory.JobBatches
                .Where(jobBatch => jobBatch.JobBatchOwnerId.Contains(ownerId))
                .OrderByDescending(jobBatch => jobBatch.CreatedAt);

            if (skip != null)
            {
                expectedJobBatches = expectedJobBatches.Skip(skip.Value);
            }

            if (limit != null)
            {
                expectedJobBatches = expectedJobBatches.Take(limit.Value);
            }

            Assert.Equal(expectedJobBatches, actualJobBatches);

            _testMemoryCacheJobBatchRepositoryFactory.MockJobBatchCache
                .Verify(
                    jobBatchCache => jobBatchCache.Query(),
                    Times.Once);
        }

        private class TestMemoryCacheJobBatchRepositoryFactory
        {
            private static readonly Random _random = new Random();

            public static ReadOnlyCollection<string> OwnerIds { get; } = Enumerable.Range(1, 5)
                .Select(number => $"owner-id-{number}")
                .ToList()
                .AsReadOnly();

            public static ReadOnlyCollection<JobBatch> JobBatches { get; } = Enumerable.Range(1, 100)
                .Select(number => new JobBatch
                {
                    JobBatchId = Guid.NewGuid(),
                    JobBatchDescription = $"JobBatch #{number}",
                    JobBatchOwnerId = OwnerIds[number % OwnerIds.Count],
                    CreatedAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(_random.NextDouble() * 60 * 24))
                })
                .ToList()
                .AsReadOnly();

            public Mock<ICache<Guid, JobBatch>> MockJobBatchCache { get; } = new Mock<ICache<Guid, JobBatch>>();

            public MemoryCacheJobBatchRepository CreateMemoryCacheJobBatchRepository()
                => new MemoryCacheJobBatchRepository(
                    NullLogger<MemoryCacheJobBatchRepository>.Instance,
                    MockJobBatchCache.Object);
        }
    }
}
