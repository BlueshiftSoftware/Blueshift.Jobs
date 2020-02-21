using System;
using System.Collections.Generic;
using System.Text;

namespace Blueshift.Jobs.DomainModel.SearchCriteria
{
    public abstract class SearchCriteriaBase
    {
        public int? MaximumItems { get; set; }

        public int? ItemsToSkip { get; set; }
    }
}
