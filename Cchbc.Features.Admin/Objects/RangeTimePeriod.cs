using System;

namespace Cchbc.Features.Admin.Objects
{
    public sealed class RangeTimePeriod
    {
        public DateTime FromDate { get; }
        public DateTime ToDate { get; }

        public RangeTimePeriod(DateTime fromDate, DateTime toDate)
        {
            this.FromDate = fromDate;
            this.ToDate = toDate;
        }
    }
}