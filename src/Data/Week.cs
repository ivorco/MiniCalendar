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
        public Week(List<Event> allAppointments = null)
        {
            foreach (var day in Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(day => day.GetThisWeekday()))
            {
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
