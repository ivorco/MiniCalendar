using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
