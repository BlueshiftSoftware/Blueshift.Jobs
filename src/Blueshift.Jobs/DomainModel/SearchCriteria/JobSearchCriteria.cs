using System;
using System.Collections.Generic;

namespace Blueshift.Jobs.DomainModel.SearchCriteria
{
    public class JobSearchCriteria : SearchCriteriaBase
    {
        public string JobOwnerId { get; set; }

        public DateTimeOffset? ExecuteAfter { get; set; }

        public ISet<JobStatus> JobStatuses { get; } = new HashSet<JobStatus>();
    }
}
