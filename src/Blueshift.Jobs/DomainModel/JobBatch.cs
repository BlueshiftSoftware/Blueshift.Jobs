using System;
using System.Collections.Generic;

namespace Blueshift.Jobs.DomainModel
{
    public class JobBatch
    {
        public JobBatch() { }

        public JobBatch(params Job[] jobs)
            : this((IEnumerable<Job>) jobs)
        {
        }

        public JobBatch(IEnumerable<Job> jobs)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException(nameof(jobs));
            }

            foreach (Job job in jobs)
            {
                Jobs.Add(job);
            }
        }

        public Guid JobBatchId { get; set; }

        public string JobBatchDescription { get; set; }

        public string JobBatchOwner { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ISet<Job> Jobs { get; } = new HashSet<Job>();
    }
}
