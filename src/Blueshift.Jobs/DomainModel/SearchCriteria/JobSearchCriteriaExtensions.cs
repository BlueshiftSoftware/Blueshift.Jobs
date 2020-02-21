using System;

namespace Blueshift.Jobs.DomainModel.SearchCriteria
{
    public static class JobSearchCriteriaExtensions
    {
        public static JobSearchCriteria WithJobStatuses(this JobSearchCriteria jobSearchCriteria, params JobStatus[] jobStatuses)
        {
            if (jobSearchCriteria == null)
            {
                throw new ArgumentNullException(nameof(jobSearchCriteria));
            }

            if (jobStatuses == null)
            {
                throw new ArgumentNullException(nameof(jobStatuses));
            }

            foreach (JobStatus jobStatus in jobStatuses)
            {
                jobSearchCriteria.JobStatuses.Add(jobStatus);
            }

            return jobSearchCriteria;
        }
    }
}
