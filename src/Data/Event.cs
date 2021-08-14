using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar.Data
{
    public struct Event
    {
        public string Subject { get; set; }
        public bool IsRightToLeft { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public EventType Type { get; set; }
        public string ID { get; set; }

        private static bool IsStringRTL(string theString)
        {
            if (string.IsNullOrWhiteSpace(theString))
                return false;

            var firstLetter = theString[0].ToString();

            var isHeberw = Regex.IsMatch(firstLetter, @"\p{IsHebrew}");
            var isArabic = Regex.IsMatch(firstLetter, @"\p{IsArabic}");

            return isHeberw || isArabic;
        }

        public static Event FromOutlook(Outlook.AppointmentItem appointmentItem)
        {
            var subject = appointmentItem.Subject;

            return new Event { ID = appointmentItem.EntryID, Type = EventType.Appointment, Subject = subject, Start = appointmentItem.Start, End = appointmentItem.End, IsRightToLeft = IsStringRTL(subject) };
        }

        public static Event FromOutlook(Outlook.TaskItem taskItem)
        {
            var subject = taskItem.Subject;

            return new Event { ID = taskItem.EntryID, Type = EventType.Task, Subject = subject, Start = taskItem.ReminderTime, End = null, IsRightToLeft = IsStringRTL(subject) };
        }
    }

    public enum EventType
    {
        Appointment,
        Task
    }
}