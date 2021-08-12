using Caliburn.Micro;
using MiniCalendar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        // TODO: Highlight upcoming appoinments
        // TODO: Installer
        // TODO: Dark mode with selector - Remeber dark mode 
        // TODO: Localization - First day of the week, dates, remove Hebrew text, hours

        public MainViewModel()
        {
            var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
#if DEBUG
            timer.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;
#endif
            timer.Elapsed += (s, e) => RefreshData();
            timer.Start();

            RefreshData();
        }

        private bool isRefreshing = false;
        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set
            {
                isRefreshing = value;
                NotifyOfPropertyChange(() => isRefreshing);
            }
        }

        private Week week = new Week();
        public Week Week
        {
            get { return week; }
            set
            {
                week = value;
                NotifyOfPropertyChange(() => week);
            }
        }

        async public void RefreshData()
        {
            if (!IsRefreshing)
            {
                IsRefreshing = true;

                await Task.Run(() =>
                 {
                     var items = GetWeeklyCalendarItems();
                     Week = new Week(items.ToList());
                 });

                IsRefreshing = false;
            }
        }

        public IEnumerable<Appointment> GetWeeklyCalendarItems()
        {
            var weekStart = DayOfWeek.Sunday.GetThisWeekday();
            var weekEnd = DayOfWeek.Saturday.GetThisWeekday();

            Outlook.Application oApp;
            Outlook.NameSpace mapiNamespace;
            Outlook.MAPIFolder CalendarFolder;
            Outlook.Items outlookCalendarItems;

            oApp = new Outlook.Application();
            mapiNamespace = oApp.GetNamespace("MAPI"); ;
            CalendarFolder = mapiNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            outlookCalendarItems = CalendarFolder.Items.Restrict($"[Start] >= '{weekStart.ToShortDateString()}' AND [End] <= '{weekEnd.ToShortDateString()}'");
            outlookCalendarItems.IncludeRecurrences = true;

            foreach (Outlook.AppointmentItem item in outlookCalendarItems)
            {
                if (item.IsRecurring)
                {
                    Outlook.RecurrencePattern rp = item.GetRecurrencePattern();
                    Outlook.AppointmentItem recur;

                    for (DateTime cur = weekStart; cur <= weekEnd; cur = cur.AddDays(1))
                    {
                        recur = null;

                        try
                        {
                            recur = rp.GetOccurrence(cur);
                        }
                        catch { }

                        if (recur != null)
                            yield return Appointment.FromOutlook(recur);
                    }
                }
                else
                {
                    yield return Appointment.FromOutlook(item);
                }
            }
        }
    }
}