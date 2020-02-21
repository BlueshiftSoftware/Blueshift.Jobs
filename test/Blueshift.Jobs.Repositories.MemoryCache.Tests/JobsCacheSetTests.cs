using Xunit;

namespace Blueshift.Jobs.Repositories.MemoryCache.Tests
{
    public class JobsCacheSetTests
    {
        [Fact]
        public void JobsCacheSet_caches_are_not_null()
        {
            var jobCacheSet = new JobsCacheSet();

            Assert.NotNull(jobCacheSet.Jobs);
            Assert.NotNull(jobCacheSet.JobBatches);
        }
    }
}
