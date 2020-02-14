using System.Collections.Generic;
using Blueshift.Jobs.DomainModel;

namespace Blueshift.Jobs.DomainModel.SearchCriteria
{
    public class JobSearchCriteria
    {
        public string OwnerName { get; set; }

        public ISet<JobStatus> JobStatuses { get; } = new HashSet<JobStatus>();

        public int MaximumJobCount { get; set; }

        public int JobsToSkip { get; set; }
    }
}
