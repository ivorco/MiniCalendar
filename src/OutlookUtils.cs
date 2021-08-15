using MiniCalendar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar
{
    public static class OutlookUtils
    {

        public static Outlook.NameSpace GetOutlookNameSpace()
        {
            Outlook.Application oApp;
            Outlook.NameSpace mapiNamespace;

            oApp = new Outlook.Application();
            mapiNamespace = oApp.GetNamespace("MAPI");

            return mapiNamespace;
        }

        public static IEnumerable<Event> GetCalendarItems(Outlook.NameSpace mapiNamespace, DateTime start, DateTime end)
        {
            Outlook.MAPIFolder calendarFolder;
            Outlook.Items outlookCalendarItems;

            calendarFolder = mapiNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            outlookCalendarItems = calendarFolder.Items.Restrict($"[Start] >= '{start.ToShortDateString()}' AND [End] <= '{end.ToShortDateString()}'");
            outlookCalendarItems.IncludeRecurrences = true;

            foreach (Outlook.AppointmentItem item in outlookCalendarItems)
            {
                if (item.IsRecurring)
                {
                    Outlook.RecurrencePattern rp = item.GetRecurrencePattern();
                    Outlook.AppointmentItem recur;

                    for (DateTime cur = start; cur <= end; cur = cur.AddDays(1))
                    {
                        recur = null;

                        try
                        {
                            recur = rp.GetOccurrence(cur);
                        }
                        catch { }

                        if (recur != null)
                            yield return Event.FromOutlook(recur);
                    }
                }
                else
                {
                    yield return Event.FromOutlook(item);
                }
            }
        }

        public static IEnumerable<Event> GetTasksItems(Outlook.NameSpace mapiNamespace, DateTime start, DateTime end)
        {
            Outlook.MAPIFolder tasksFolder;
            Outlook.Items outlookTasksItems;

            tasksFolder = mapiNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderTasks);
            outlookTasksItems = tasksFolder.Items.Restrict($"[ReminderTime] >= '{start.ToShortDateString()}' AND [ReminderTime] <= '{end.ToShortDateString()}' AND [Complete] = False");
            outlookTasksItems.IncludeRecurrences = true;

            return outlookTasksItems.OfType<Outlook.TaskItem>().Select(Event.FromOutlook);
        }

        public static void DisplayEvent(string eventId)
        {
            var ons = GetOutlookNameSpace();
            var eventItem = ons.GetItemFromID(eventId);
            if (eventItem is Outlook.AppointmentItem appt)
                appt.Display(true);
            else if (eventItem is Outlook.TaskItem task)
                task.Display(true);
        }

        public static void AddTask(DateTime date, string subject)
        {
            var ons = GetOutlookNameSpace();
            var this10AM = date.Date + TimeSpan.FromHours(10);
            Outlook.TaskItem item = ons.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderTasks).Items.Add() as Outlook.TaskItem;
            item.Subject = subject;
            item.ReminderSet = true;
            item.ReminderTime = this10AM;
            item.Display();
        }
    }
}
