using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace MiniCalendar.Data
{
    public class Week : BindableCollection<Day>
    {
        public Week() { }

        public Week(DateTime start, DateTime end, List<Event> allEvents = null)
        {
            var days = (end - start).TotalDays;

            for (int d = 0; d <= days; d++)
            {
                var day = start.AddDays(d);

                Add(new Day { Date = day, Events = allEvents?.Where(app => app.Start.Date == day.Date)?.ToList() ?? new List<Event>() });
            }
        }

        public IEnumerable<Event> AllEvents()
        {
            return this.SelectMany(day => day.Events);
        }
    }
}
