namespace Blueshift.Jobs.DomainModel
{
    public enum JobStatus
    {
        None = 0,

        Created = 1,

        Pending = 2,

        Completed = 3,

        Failed = 4,

        Cancelled = 5
    }
}
