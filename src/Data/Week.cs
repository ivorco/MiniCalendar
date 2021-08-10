using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCalendar.Data
{
    public class Week : Dictionary<DateTime, List<Appointment>>
    {
        public Week(List<Appointment> allAppointments = null)
        {
            foreach (var day in Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(day => day.GetThisWeekday()))
            {
                List<Appointment> dayAppointments;

                if (allAppointments == null)
                    dayAppointments = new List<Appointment>();
                else
                    dayAppointments = allAppointments.Where(app => app.Start.Date == day.Date).ToList();

                Add(day, dayAppointments);
            }
        }
    }
}
