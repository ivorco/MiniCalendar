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

        public static void DisplayItem(string itemId)
        {
            var ons = GetOutlookNameSpace();
            var item = ons.GetItemFromID(itemId);
            if (item is Outlook.AppointmentItem appt)
                appt.Display(true);
            else if (item is Outlook.TaskItem task)
                task.Display(true);
            else if (item is Outlook.MailItem email)
                email.Display(true);
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

        public static void AddAppointment(DateTime date, string subject)
        {
            var ons = GetOutlookNameSpace();
            var this10AM = date.Date + TimeSpan.FromHours(10);
            var this12AM = date.Date + TimeSpan.FromHours(12);
            Outlook.AppointmentItem item = ons.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar).Items.Add() as Outlook.AppointmentItem;
            item.Subject = subject;
            item.ReminderSet = true;
            item.Start = this10AM;
            item.End = this12AM;
            item.Display();
        }

        public static IEnumerable<MailItem> GetMailItems(Outlook.NameSpace mapiNamespace, bool flagged)
        {
            IEnumerable<Outlook.MAPIFolder> mailFolders;
            IEnumerable<Outlook.MailItem> outlookMailItems;

            mailFolders = mapiNamespace.Stores.OfType<Outlook.Store>().Select(store => store.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox));

            if (flagged)
                outlookMailItems = mailFolders.SelectMany(folder => folder.Items.Restrict("[FlagStatus] > 1").OfType<Outlook.MailItem>());
            else
                outlookMailItems = mailFolders.SelectMany(folder => folder.Items.OfType<Outlook.MailItem>());

            return outlookMailItems.Select(MailItem.FromOutlook);
        }
    }
}
