using System;

namespace Blueshift.Jobs.DomainModel
{
    public class JobStatusChangeEvent
    {
        public JobStatus PreviousStatus { get; set; }

        public JobStatus NewStatus { get; set; }

        public DateTimeOffset StatusChangedAt { get; set; }

        public string Description { get; set; }
    }
}
