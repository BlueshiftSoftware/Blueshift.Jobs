namespace Blueshift.Jobs.DomainModel.SearchCriteria
{
    public class JobBatchSearchCriteria
    {
        public string OwnerName { get; set; }

        public int MaximumJobBatchCount { get; set; }

        public int JobBatchesToSkip { get; set; }
    }
}
