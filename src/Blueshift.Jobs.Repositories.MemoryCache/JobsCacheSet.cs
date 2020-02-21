using System;
using Blueshift.Jobs.DomainModel;

namespace Blueshift.Jobs.Repositories.MemoryCache
{
    public class JobsCacheSet
    {
        public ICache<Guid, JobBatch> JobBatches { get; } = new ConcurrentCache<Guid, JobBatch>();

        public ICache<Guid, Job> Jobs { get; } = new ConcurrentCache<Guid, Job>();
    }
}
