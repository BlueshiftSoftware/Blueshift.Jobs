using System;
using System.Collections.Generic;

namespace Blueshift.Jobs.DomainModel
{
    public class Job
    {
        public Guid JobId { get; set; }

        public string JobOwnerId { get; set; }

        public string JobDescription { get; set; }

        public JobStatus JobStatus { get; set; }

        public ISet<JobStatusChangeEvent> JobStatusChangeEvents { get; } = new HashSet<JobStatusChangeEvent>();

        public IDictionary<string, string> JobParameters { get; } = new Dictionary<string, string>();
    }
}
