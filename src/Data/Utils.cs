using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniCalendar.Data
{
    public static class Utils
    {
        public static DateTime GetThisWeekday(this DayOfWeek day, DateTime? start = null)
        {
            if (start == null)
                start = DateTime.Now;

            return start.Value.AddDays((int)day - (int)start.Value.DayOfWeek);
        }

        public static DateTime? GetTimeFromString(string text)
        {
            // HH:MM 12-hour format, optional leading 0, mandatory meridiems (AM/PM)
            var timeRegex = new Regex(@"\b((1[0-2]|0?[1-9]):([0-5][0-9]) ([AaPp][Mm]))");
            var timeMatch = timeRegex.Match(text);

            if (timeMatch.Success)
                return DateTime.Parse(timeMatch.Value);


            // HH:MM 24-hour format, optional leading 0
            timeRegex = new Regex(@"\b([0-9]|0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]");
            timeMatch = timeRegex.Match(text);

            if (timeMatch.Success)
                return DateTime.Parse(timeMatch.Value);

            return null;
        }
    }
}
