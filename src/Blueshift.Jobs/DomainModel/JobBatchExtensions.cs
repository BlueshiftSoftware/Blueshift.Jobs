using System;
using System.Collections.Generic;

namespace Blueshift.Jobs.DomainModel
{
    public static class JobBatchExtensions
    {
        public static JobBatch WithJobs(this JobBatch jobBatch, IEnumerable<Job> jobs)
        {
            if (jobBatch == null)
            {
                throw new ArgumentNullException(nameof(jobBatch));
            }
            if (jobs == null)
            {
                throw new ArgumentNullException(nameof(jobs));
            }

            foreach (Job job in jobs)
            {
                jobBatch.Jobs.Add(job);
            }

            return jobBatch;
        }
    }
}
