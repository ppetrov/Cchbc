using System;

namespace Cchbc.Features.Admin.Objects
{
    public sealed class TimePeriod
    {
        public DateTime FromDate { get; }
        public DateTime ToDate { get; }

        public TimePeriod(DateTime fromDate, DateTime toDate)
        {
            this.FromDate = fromDate;
            this.ToDate = toDate;
        }
    }
}