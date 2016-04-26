﻿using System;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.ViewModels;

namespace Cchbc.Features.Admin.Helpers
{
    public static class TimePeriodHelper
    {
        public static TimePeriodViewModel[] GetStandardPeriods()
        {
            var today = DateTime.Today;
            var toDate = today.AddDays(1);
            return new[]
            {
                new TimePeriodViewModel(@"Today", new TimePeriod(today, toDate)),
                new TimePeriodViewModel(@"Last 7 days", new TimePeriod(toDate.AddDays(-7), toDate)),
                new TimePeriodViewModel(@"Last 30 days", new TimePeriod(toDate.AddDays(-30), toDate)),
            };
        }
    }
}