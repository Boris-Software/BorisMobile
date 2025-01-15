using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Utilities
{
    public class DateTimeRangeCollection : Collection<DateTimeRange>
    {
        public int TotalIntervalSeconds
        {
            get
            {
                int total = 0;
                foreach (DateTimeRange range in Items)
                {
                    total += range.IntervalSeconds;
                }
                return total;
            }
        }

        //public DateTime IncrementHoursUntilOutsideOfRanges(DateTime baseDate)
        //{
        //    bool restartRanges = true;
        //    while (restartRanges)
        //    {
        //        restartRanges = false;
        //        foreach (DateTimeRange oneRange in this)
        //        {
        //            if (oneRange.TimeIsWithinDateRange(baseDate))
        //            {
        //                baseDate = baseDate.AddHours(1);
        //                restartRanges = true;
        //                break;
        //            }
        //        }
        //    }
        //    return baseDate;
        //}
    }
}
