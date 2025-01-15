using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Utilities
{
    public class Chrono
    {
        public Chrono()
        {
        }

        public static DateTime DateTimeAddOffset(DateTime baseDate, string offsetDefinition)
        {
            return DateTimeAddOffset(baseDate, offsetDefinition, null);
        }

        public static DateTime DateTimeAddOffset(DateTime baseDate, string offsetDefinition, Collection<DateTime> holidays)
        {
            if (!string.IsNullOrEmpty(offsetDefinition))
            {
                int daysAdd = 0;
                if (Int32.TryParse(offsetDefinition, out daysAdd))
                {
                    return baseDate.AddDays(daysAdd);
                }
                else if (offsetDefinition.EndsWith("Y") && offsetDefinition != "Y") // add years
                {
                    string numYearsStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int yearsAdd = 0;
                    if (Int32.TryParse(numYearsStr, out yearsAdd))
                    {
                        return baseDate.AddYears(yearsAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("M") && offsetDefinition != "M") // add months
                {
                    string numMonthsStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int monthsAdd = 0;
                    if (Int32.TryParse(numMonthsStr, out monthsAdd))
                    {
                        return baseDate.AddMonths(monthsAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("D") && offsetDefinition != "D") // add days
                {
                    string numDaysStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int dAdd = 0;
                    if (Int32.TryParse(numDaysStr, out dAdd))
                    {
                        return baseDate.AddDays(dAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("W") && offsetDefinition != "W") // add weeks // 130520, had missed this out, caused an issue at WWL with weekly tasks!
                {
                    string numWeeksStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int wAdd = 0;
                    if (Int32.TryParse(numWeeksStr, out wAdd))
                    {
                        return baseDate.AddDays(7 * wAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("DXW") && offsetDefinition != "DXW") // add days excluding weekends
                {
                    string numDaysStr = offsetDefinition.Substring(0, offsetDefinition.Length - 3);
                    if (Int32.TryParse(numDaysStr, out daysAdd))
                    {
                        return AddWorkingDays(baseDate, daysAdd, true, null);
                    }
                }
                else if (offsetDefinition.EndsWith("DXH") && offsetDefinition != "DXH") // add days excluding holidays
                {
                    string numDaysStr = offsetDefinition.Substring(0, offsetDefinition.Length - 3);
                    if (Int32.TryParse(numDaysStr, out daysAdd))
                    {
                        return AddWorkingDays(baseDate, daysAdd, false, holidays);
                    }
                }
                else if (offsetDefinition.EndsWith("DXWH") && offsetDefinition != "DXWH") // add days excluding weekends and holidays
                {
                    string numDaysStr = offsetDefinition.Substring(0, offsetDefinition.Length - 4);
                    if (Int32.TryParse(numDaysStr, out daysAdd))
                    {
                        return AddWorkingDays(baseDate, daysAdd, true, holidays);
                    }
                }
                else if (offsetDefinition.EndsWith("H") && offsetDefinition != "H") // add hours
                {
                    string numHoursStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int hoursAdd = 0;
                    if (Int32.TryParse(numHoursStr, out hoursAdd))
                    {
                        return baseDate.AddHours(hoursAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("S") && offsetDefinition != "S") // add seconds
                {
                    string numSecsStr = offsetDefinition.Substring(0, offsetDefinition.Length - 1);
                    int secsAdd = 0;
                    if (Int32.TryParse(numSecsStr, out secsAdd))
                    {
                        return baseDate.AddSeconds(secsAdd);
                    }
                }
                else if (offsetDefinition.EndsWith("MIN") && offsetDefinition != "MIN") // add mins
                {
                    string numMinsStr = offsetDefinition.Substring(0, offsetDefinition.Length - 3);
                    int minsAdd = 0;
                    if (Int32.TryParse(numMinsStr, out minsAdd))
                    {
                        return baseDate.AddMinutes(minsAdd);
                    }
                }
                else
                {
                    // WWL 071019 could be adding hours like 8HX0000080016000000
                    string[] bits = offsetDefinition.Split(new string[] { "HX" }, StringSplitOptions.None);
                    if (bits.Length == 2)
                    {
                        int numHoursToAdd = -1;
                        if (Int32.TryParse(bits[0], out numHoursToAdd))
                        {
                            string timesToExclude = bits[1];
                            if (timesToExclude.Length % 8 == 0) // make sure hours are specified correctly
                            {
                                // Use minutes to make it easier to extend to "add minutes" and also to make clear that "add 24 hours" isn't "add 1 day"
                                int numMinsToAdd = numHoursToAdd * 60;
                                int totalMinsInRanges = 0;
                                DateTimeRangeCollection timeRanges = new DateTimeRangeCollection();
                                for (int timeRange = 0; timeRange < (timesToExclude.Length / 8); timeRange++)
                                {
                                    string oneRangeAsString = timesToExclude.Substring(timeRange * 8, 8);
                                    string startTimeString = oneRangeAsString.Substring(0, 4);
                                    string endTimeString = oneRangeAsString.Substring(4, 4);
                                    DateTime startDate = Chrono.DayBeginningForDate(baseDate).AddHours(Int32.Parse(startTimeString.Substring(0, 2))).AddMinutes(Int32.Parse(startTimeString.Substring(2, 2)));
                                    DateTime endDate = Chrono.DayBeginningForDate(baseDate);
                                    if (endTimeString == "0000")
                                    {
                                        endDate = endDate.AddDays(1);
                                    }
                                    else
                                    {
                                        endDate = endDate.AddHours(Int32.Parse(endTimeString.Substring(0, 2))).AddMinutes(Int32.Parse(endTimeString.Substring(2, 2)));
                                    }
                                    DateTimeRange range = new DateTimeRange(startDate, endDate);
                                    timeRanges.Add(range);
                                    totalMinsInRanges += range.IntervalMinutes;
                                }
                                int availableMinsInDay = 60 * 24 - totalMinsInRanges;
                                if (numMinsToAdd >= availableMinsInDay)
                                {
                                    baseDate = baseDate.AddDays(numMinsToAdd / availableMinsInDay);
                                    numMinsToAdd = numMinsToAdd % availableMinsInDay;
                                }
                                // Always check the overlaps, even if we have 0 mins to add (could be a start time in the evening)
                                DateTime fullbaseDate = baseDate.AddMinutes(numMinsToAdd);
                                // baseDate and fullBaseDate are no more than 24 hours apart
                                // Now add in hours of overlap with any of the date ranges
                                int overlapMins = 0;
                                for (int i = 0; i < timeRanges.Count; i++)
                                {
                                    DateTimeRange oneRange = timeRanges[i];
                                    if (MinsInDay(baseDate) < MinsInDay(oneRange.StartDateTime)) // start date is before the start of the range
                                    {
                                        if (MinsInDay(fullbaseDate) > MinsInDay(oneRange.StartDateTime))
                                        {
                                            if (MinsInDay(oneRange.FinishDateTime) == 0) // goes to midnight, need all of this range plus the range from midnight, if it exists
                                            {
                                                overlapMins += (int)(oneRange.FinishDateTime - oneRange.StartDateTime).TotalMinutes;
                                                if (i > 0 && i == timeRanges.Count - 1)
                                                {
                                                    DateTimeRange fromMidnightMaybe = timeRanges[0];
                                                    if (MinsInDay(fromMidnightMaybe.StartDateTime) == 0)
                                                    {
                                                        overlapMins += (int)(fromMidnightMaybe.FinishDateTime - fromMidnightMaybe.StartDateTime).TotalMinutes;
                                                    }
                                                }
                                            }
                                            else if (MinsInDay(fullbaseDate) == 0 || MinsInDay(fullbaseDate) > MinsInDay(oneRange.FinishDateTime))
                                            {
                                                overlapMins += (int)(oneRange.FinishDateTime - oneRange.StartDateTime).TotalMinutes;
                                            }
                                            else // if (MinsInDay(fullbaseDate) > MinsInDay(oneRange.StartDateTime))
                                            {
                                                overlapMins += (MinsInDay(fullbaseDate) - MinsInDay(oneRange.StartDateTime));
                                            }
                                        }
                                        else
                                        {
                                            // start and end of period is before the date range
                                            overlapMins += 0;
                                        }
                                    }
                                    else if (MinsInDay(baseDate) >= MinsInDay(oneRange.FinishDateTime)) // start date is after the end of the range
                                    {
                                        overlapMins += 0;
                                    }
                                    else // start date is within this range
                                    {
                                        if (MinsInDay(fullbaseDate) == 0 || MinsInDay(fullbaseDate) > MinsInDay(oneRange.FinishDateTime))
                                        {
                                            overlapMins += (int)(oneRange.FinishDateTime - baseDate).TotalMinutes;
                                        }
                                        else if (MinsInDay(fullbaseDate) > MinsInDay(oneRange.StartDateTime))
                                        {
                                            overlapMins += (MinsInDay(fullbaseDate) - MinsInDay(baseDate));
                                        }
                                        else
                                        {
                                            // shouldn't happen
                                            overlapMins += 0;
                                        }
                                    }
                                }
                                baseDate = fullbaseDate.AddMinutes(overlapMins);

                            }
                        }
                        //int numHoursToAdd = -1;
                        //if (Int32.TryParse(bits[0], out numHoursToAdd))
                        //{
                        //    string timesToExclude = bits[1];
                        //    if (timesToExclude.Length % 8 == 0) // make sure hours are specified correctly
                        //    {
                        //        DateTimeRangeCollection timeRanges = new DateTimeRangeCollection();
                        //        for (int timeRange = 0; timeRange < (timesToExclude.Length / 8); timeRange++)
                        //        {
                        //            string oneRangeAsString = timesToExclude.Substring(timeRange * 8, 8);
                        //            string startTimeString = oneRangeAsString.Substring(0, 4);
                        //            string endTimeString = oneRangeAsString.Substring(4, 4);
                        //            DateTime startDate = Chrono.DayBeginningForDate(baseDate).AddHours(Int32.Parse(startTimeString.Substring(0, 2))).AddMinutes(Int32.Parse(startTimeString.Substring(2, 2)));
                        //            DateTime endDate = Chrono.DayBeginningForDate(baseDate).AddHours(Int32.Parse(endTimeString.Substring(0, 2))).AddMinutes(Int32.Parse(endTimeString.Substring(2, 2)));
                        //            if (endDate > startDate)
                        //            {
                        //                DateTimeRange range = new DateTimeRange(startDate, endDate);
                        //                timeRanges.Add(range);
                        //            }
                        //        }
                        //        // TO DO 311019
                        //        //while (numHoursToAdd > 0)
                        //        //{
                        //        //    baseDate = baseDate.AddHours(1);
                        //        //    baseDate = timeRanges.IncrementHoursUntilOutsideOfRanges(baseDate);
                        //        //    numHoursToAdd--;
                        //        //}
                        //    }
                        //}
                    }
                }
            }
            return baseDate;
        }

        public static int MinsInDay(DateTime dateTime)
        {
            return (dateTime.Hour * 60) + dateTime.Minute;
        }

        public static DateTime WeekBeginningForDate(DateTime origDateTime, DayOfWeek startOfWeek /* 0 for Sunday, 1 for Monday*/)
        {
            DateTime dateTime = DayBeginningForDate(origDateTime);
            if (dateTime.DayOfWeek == startOfWeek)
            {
                return dateTime;
            }
            else if (dateTime.DayOfWeek > startOfWeek)
            {
                return dateTime.AddDays(startOfWeek - dateTime.DayOfWeek);
            }
            else // DayOfWeek < startOfWeek (0 & 1)
            {
                return dateTime.AddDays(-(7 - (startOfWeek - dateTime.DayOfWeek)));
            }
        }

        public static DateTime WeekEndingForDate(DateTime origDateTime, DayOfWeek startOfWeek /* 0 for Sunday, 1 for Monday*/)
        {
            DateTime weekStartTime = WeekBeginningForDate(origDateTime, startOfWeek);
            return weekStartTime.AddDays(7).AddMinutes(-1);
        }

        public static string ConvertDateTimeToISOString(DateTime dateTime)
        {
            return dateTime.ToString("s");
        }

        public static string ISOStartOfWeek(DateTime dateTime, DayOfWeek startOfWeek)
        {
            DateTime dateTimeStartOfWeek = WeekBeginningForDate(dateTime, startOfWeek);
            return ConvertDateTimeToISOString(dateTimeStartOfWeek);
        }

        public static string ISOStartOfDay(DateTime dateTime)
        {
            DateTime dateTimeStartOfDay = DayBeginningForDate(dateTime);
            return ConvertDateTimeToISOString(dateTimeStartOfDay);
        }

        public static DateTime DayEndForDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        public static DateTime DayBeginningForDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public static DateTime MonthBeginningForDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
        }

        public static DateTime YearBeginningForDate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1, 0, 0, 0);
        }

        public static int GetMinutesFromTime(string text)
        {
            string[] hoursMins = text.Split(':');
            if (hoursMins.Length == 2 && !string.IsNullOrEmpty(hoursMins[0]) && !string.IsNullOrEmpty(hoursMins[1]))
            {
                return (Int32.Parse(hoursMins[0]) * 60) + Int32.Parse(hoursMins[1]);
            }
            else if (hoursMins.Length == 1 && !string.IsNullOrEmpty(hoursMins[0])) // 06/05/17, be lenient, hope this is OK
            {
                int h = Int32.Parse(hoursMins[0]);
                if (h != -1)
                {
                    return h * 60;
                }
            }
            return -1;
        }

        public static TimeSpan TimeSpanFromString(string stringValue)
        {
            int hours = 0;
            int mins = 0;
            int secs = 0;
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] bits = stringValue.Split(':');
                if (bits.Length > 0)
                {
#if !PocketPC
                    Int32.TryParse(bits[0], out hours);
#else
                    hours = Int32.Parse(bits[0]);
#endif
                }
                if (bits.Length > 1)
                {
#if !PocketPC
                    Int32.TryParse(bits[1], out mins);
#else
                    mins = Int32.Parse(bits[1]);
#endif
                }
                if (bits.Length > 2)
                {
#if !PocketPC
                    Int32.TryParse(bits[2], out secs);
#else
                    secs = Int32.Parse(bits[2]);
#endif
                }
            }
            return new TimeSpan(hours, mins, secs);
        }

#if !ClientBuild
        public static string ISODateFromString(string dateString, CultureInfo culture)
        {
            //DateTime resDateTime = DateTime.Now;
            //if (DateTime.TryParseExact(dateString, General.InputDateFormats, culture, DateTimeStyles.AllowWhiteSpaces, out resDateTime))
            //{
            //    return ConvertDateTimeToISOString(resDateTime);
            //}
            return "";
        }

        public static DateTime DateFromString(string dateString, CultureInfo culture)
        {
            //DateTime resDateTime = DateTime.Now;
            //if (DateTime.TryParseExact(dateString, General.InputDateFormats, culture, DateTimeStyles.AllowWhiteSpaces, out resDateTime))
            //{
            //    return resDateTime;
            //}
            return DateTime.MinValue;
        }
#endif

        public static bool DateIsEmpty(DateTime dateTime)
        {
            return dateTime == SqlMinDate() || dateTime == DateTime.MinValue || dateTime == SqlMinAppSmallDateTime();
        }

        public static string GetTimeFromMinutes(int numMins)
        {
            if (numMins != -1)
            {
                return string.Format("{0}:{1:D2}", numMins / 60, numMins % 60);
            }
            return "";
        }

        public static string GetTimeFromSeconds(int numSecs)
        {
            if (numSecs != -1)
            {
                string hhmmss = string.Format("{0}:{1:D2}", GetTimeFromMinutes(numSecs / 60), numSecs % 60);
                if (hhmmss.StartsWith("0:"))
                {
                    return hhmmss.Substring(2);
                }
                return hhmmss;
            }
            return "";
        }

        public static decimal GetTimeAsDecimal(int numMins)
        {
            if (numMins != -1)
            {
                return (decimal)numMins / 60M;
            }
            return -1;
        }

        public static string ConvertDateTimeToShortDateAndTime(DateTime dateTime)
        {
#if ClientBuild
            // Just because MonoDroid mangles things with ToShortDateString (includes year)
            return Misc.ConvertDateTimeToShortDateNoTime(dateTime) + " " + dateTime.ToShortTimeString();
#else
            return dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString();
#endif
        }

        private static DateTime SqlMinDate()
        {
            return new DateTime(1753, 1, 1);
        }

        public static DateTime SqlMinAppSmallDateTime()
        {
            return new DateTime(2007, 1, 1);
        }

        public static string TimeOrUnknown(DateTime dateTime)
        {
            if (!Chrono.DateIsEmpty(dateTime))
            {
                return Chrono.ConvertDateTimeToTimeOnlyHHMM(dateTime);
            }
            return "??:??";
        }

        public static string DateTimeOrUnknown(DateTime dateTime)
        {
            if (!Chrono.DateIsEmpty(dateTime))
            {
                return ConvertDateTimeToShortDateAndTime(dateTime);
            }
            return "???";
        }

        public static string ConvertDateTimeToTimeOnlyHHMM(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        public static DateTime AddWorkingDays(DateTime startDate, int workingDaysToAdd, bool skipWeekends, Collection<DateTime> holidaysToSkip)
        {
            int direction = workingDaysToAdd < 0 ? -1 : 1;
            DateTime newDate = DayBeginningForDate(startDate);
            while (workingDaysToAdd != 0)
            {
                newDate = newDate.AddDays(direction);
                if (
                        ((newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday) || !skipWeekends)
                        &&
                        (holidaysToSkip == null || !holidaysToSkip.Contains(newDate))
                    )
                {
                    workingDaysToAdd -= direction;
                }
            }
            return newDate;
        }

        public static int WorkingDaysOffset(DateTime endDate, DateTime startDate, bool skipWeekends, Collection<DateTime> holidaysToSkip)
        {
            if (endDate.Date == startDate.Date)
            {
                return 0;
            }
            int direction = (endDate > startDate) ? 1 : -1;
            int numDaysOffset = 0;
            DateTime newDate = DayBeginningForDate(startDate);
            while (newDate.Date != endDate.Date)
            {
                newDate = newDate.AddDays(direction);
                if (
                        ((newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday) || !skipWeekends)
                        &&
                        (holidaysToSkip == null || !holidaysToSkip.Contains(newDate))
                    )
                {
                    numDaysOffset += direction;
                }
            }
            return numDaysOffset;
        }
    }
}
