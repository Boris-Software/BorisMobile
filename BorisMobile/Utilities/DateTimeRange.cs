using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Utilities
{
    public struct DateTimeRange
    {
        private DateTime m_startDateTime;

        public DateTime StartDateTime
        {
            get { return m_startDateTime; }
            set { m_startDateTime = value; }
        }
        private DateTime m_finishDateTime;

        public DateTime FinishDateTime
        {
            get { return m_finishDateTime; }
            set { m_finishDateTime = value; }
        }

        public DateTimeRange(DateTime startDateTime, DateTime finishDateTime)
        {
            m_startDateTime = startDateTime;
            m_finishDateTime = finishDateTime;
        }

        public bool IsEmpty
        {
            get
            {
                return Chrono.DateIsEmpty(m_startDateTime) || Chrono.DateIsEmpty(m_finishDateTime);
            }
        }

        private static DateTime SqlMinDate()
        {
            return new DateTime(1753, 1, 1);
        }

        public void Extend(DateTime start, DateTime end)
        {
            ExtendBack(start);
            ExtendForward(end);
        }

        public void ExtendForward(DateTime dateTime)
        {
            if (Chrono.DateIsEmpty(m_finishDateTime))
            {
                m_finishDateTime = dateTime;
            }
            else if (dateTime > m_finishDateTime)
            {
                m_finishDateTime = dateTime;
            }
        }

        public void ExtendBack(DateTime dateTime)
        {
            if (Chrono.DateIsEmpty(m_startDateTime))
            {
                m_startDateTime = dateTime;
            }
            else if (dateTime < m_startDateTime)
            {
                m_startDateTime = dateTime;
            }
        }

        public int IntervalSeconds
        {
            get
            {
                return (int)m_finishDateTime.Subtract(m_startDateTime).TotalSeconds;
            }
        }

        public int IntervalMinutes
        {
            get
            {
                return (int)m_finishDateTime.Subtract(m_startDateTime).TotalMinutes;
            }
        }

        public string ToString(string formatSpecifier)
        {
            StringBuilder sb = new StringBuilder();
            if (formatSpecifier == "timeonly")
            {
                sb.Append(Chrono.TimeOrUnknown(m_startDateTime));
                sb.Append(" - ");
                sb.Append(Chrono.TimeOrUnknown(m_finishDateTime));
            }
            else
            {
                sb.Append(Chrono.DateTimeOrUnknown(m_startDateTime));
                sb.Append(" - ");
                sb.Append(Chrono.DateTimeOrUnknown(m_finishDateTime));
            }
            return sb.ToString();
        }

        public int MinsInDay(DateTime dateTime)
        {
            return (dateTime.Hour * 60) + dateTime.Minute;
        }

        // Trickier than this
        //public bool TimeIsWithinDateRange(DateTime baseDate)
        //{
        //    if (MinsInDay(m_startDateTime) < MinsInDay(baseDate)
        //            &&
        //            (
        //            MinsInDay(m_finishDateTime) == 0 /* midnight at end of day */ ||
        //            MinsInDay(m_finishDateTime) > MinsInDay(baseDate)
        //            ))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
