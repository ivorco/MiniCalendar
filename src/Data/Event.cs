using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar.Data
{
    public class Event : Item
    {
        public DateTime? End { get; set; }
        public EventType Type { get; set; }

        public static Event FromOutlook(Outlook.AppointmentItem appointmentItem)
        {
            var subject = appointmentItem.Subject;

            return new Event { ID = appointmentItem.EntryID, Type = EventType.Appointment, Subject = subject, Start = appointmentItem.Start, End = appointmentItem.End, IsRightToLeft = IsStringRTL(subject) };
        }

        public static Event FromOutlook(KeyValuePair<Outlook.TaskItem, DateTime> taskAndReminder)
        {
            var subject = taskAndReminder.Key.Subject;

            return new Event { ID = taskAndReminder.Key.EntryID, Type = EventType.Task, Subject = subject, Start = taskAndReminder.Value, End = null, IsRightToLeft = IsStringRTL(subject) };
        }
    }

    public enum EventType
    {
        Appointment,
        Task
    }
}