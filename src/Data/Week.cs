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
        public Week(DateTime start, DateTime end, List<Event> allAppointments = null)
        {
            var days = (end - start).TotalDays;

            for (int d = 0; d <= days; d++)
            {
                var day = start.AddDays(d);

                List<Event> dayAppointments;

                if (allAppointments == null)
                    dayAppointments = new List<Event>();
                else
                    dayAppointments = allAppointments.Where(app => app.Start.Date == day.Date).ToList();

                Add(new Day { Date = day, Events = dayAppointments });
            }
        }
    }
}
