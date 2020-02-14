using Blueshift.Jobs.DomainModel.SearchCriteria;

namespace Blueshift.Jobs.DomainModel.Requests
{
    public class CreateJobBatchRequest
    {
        public string JobBatchDescription { get; set; }

        public string JobBatchRequestor { get; set; }

        public JobSearchCriteria BatchingJobFilter { get; } = new JobSearchCriteria();
    }
}
