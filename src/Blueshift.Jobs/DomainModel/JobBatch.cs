using System;
using System.Collections.Generic;

namespace Blueshift.Jobs.DomainModel
{
    public class JobBatch
    {
        public Guid JobBatchId { get; set; }

        public string JobBatchDescription { get; set; }

        public string JobBatchOwnerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ISet<Job> Jobs { get; } = new HashSet<Job>();
    }
}
