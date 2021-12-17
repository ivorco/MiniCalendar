using Caliburn.Micro;
using MiniCalendar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MiniCalendar.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        // Important (usage)
        // TODO: Make bottom buttons look better
        // TODO: Style context menu
        // TODO: Style delete message box
        // TODO: Resizing to narrow doesn't look good
        // TODO: When displaying an appointment, display the reccurence and not the series
        // TODO: Weekly navigation: D:\Git\MiniCalendar\CurrentWeek.png

        // The next stage
        // TODO: minical -> focus point
        // TODO: Add routine placeholder for food, dog, breakfest and more
        // TODO: Add sport plugin that finds empty spots before and after eating
        // TODO: Connect with stove/coffee machine to find out if I ate/drank

        // Nice to have
        // TODO: Don't crash when outlook isn't connected
        // TODO: Allow browsing next week and on
        // TODO: When hovering a day, adding the buttons at the bottom might create a scrollbar because items are almost filling the whole container, then the scrollbar will be pushing some almost long items, making them drop down one line, further affecting the ui
        // TODO: Move data.item to the events themselves maybe
        // TODO: Move outlook utils to item
        // TODO: Hide context menu if empty (all items collapsed)
        // TODO: Right click a task or appointment to covert to other type of event
        // TODO: Show tasks with empty or expired reminders - where?
        // TODO: Dark mode with selector - Remember dark mode, fix light mode colors
        // TODO: Properly run updates on thread and timers
        // TODO: Give thanks to: thenounproject, calibrun, TextBlockService
        // TODO: Localization - First day of the week, dates, remove Hebrew text, hours
        // TODO: Updates
        // TODO: Installer

        public MainViewModel()
        {
            if (Execute.InDesignMode)
            {
                RefreshDataDesignTime();
                return;
            }

            var mapiNamespace = OutlookUtils.GetOutlookNameSpace();
            OutlookUtils.HandleUpdateEvents(mapiNamespace, RefreshData);

            var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
#if DEBUG
            timer.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;
#endif
            timer.Elapsed += (s, e) => RefreshData();
            timer.Start();

            RefreshData();

            var timerCurrentTime = new Timer(500);
            timerCurrentTime.Elapsed += (s, e) => RefreshTime();
            timerCurrentTime.Start();
        }

        private void RefreshTime()
        {
            CurrentTime = DateTime.Now;
            SnoozeTime = SnoozeTime;

            var eventsWithin30Minutes = Week.AllEvents().Where(wevent => wevent.Start - CurrentTime < TimeSpan.FromMinutes(30) && wevent.Start - CurrentTime > TimeSpan.Zero);
            var eventsOrderedByDateAppoitmentFirst = eventsWithin30Minutes.OrderBy(wevent => (wevent.Type == EventType.Appointment ? DateTime.MinValue : wevent.Start));

            // Only show events if there is an upcoming appointment
            if (eventsOrderedByDateAppoitmentFirst.Any(eventi => eventi.Type == EventType.Appointment))
                NextEvents = new BindableCollection<Event>(eventsOrderedByDateAppoitmentFirst);
            else
                NextEvents = new BindableCollection<Event>();
        }

        public bool PauseRefresh { get; set; } = false;

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

        #region Next Events

        private bool isSnoozing = false;
        public bool IsSnoozing
        {
            get { return isSnoozing; }
            set
            {
                isSnoozing = value;
                NotifyOfPropertyChange(() => isSnoozing);
            }
        }

        private DateTime currentTime = DateTime.MinValue;
        public DateTime CurrentTime
        {
            get { return currentTime; }
            set
            {
                currentTime = value;
                NotifyOfPropertyChange(() => currentTime);
            }
        }

        private DateTime snoozeTime = DateTime.MinValue;
        public DateTime SnoozeTime
        {
            get { return snoozeTime; }
            set
            {
                snoozeTime = value;
                NotifyOfPropertyChange(() => snoozeTime);

                IsSnoozing = CurrentTime - SnoozeTime < TimeSpan.FromMinutes(5);
            }
        }

        private BindableCollection<Event> nextEvents = new BindableCollection<Event>();
        public BindableCollection<Event> NextEvents
        {
            get { return nextEvents; }
            set
            {
                nextEvents = value;
                NotifyOfPropertyChange(() => nextEvents);
            }
        }

        public void Snooze()
        {
            SnoozeTime = DateTime.Now;
        }

        #endregion

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

        private BindableCollection<MailItem> importantEMails = new BindableCollection<MailItem>();
        public BindableCollection<MailItem> ImportantEMails
        {
            get { return importantEMails; }
            set
            {
                importantEMails = value;
                NotifyOfPropertyChange(() => importantEMails);
            }
        }

        private void RefreshDataDesignTime()
        {
            var sunday = new DateTime(2021, 1, 3, 10, 0, 0);
            var monday = sunday.AddDays(1);
            var tuesday = sunday.AddDays(2);
            var wednesday = sunday.AddDays(3);
            var thursday = sunday.AddDays(4);
            var friday = sunday.AddDays(5);
            var saturday = sunday.AddDays(6);

            Week = new Week(sunday, saturday, new List<Event> {
                new Event { Start = sunday, End = sunday.AddHours(1), Type = EventType.Task, Subject = "Test" },
                new Event { Start = tuesday, End = tuesday.AddHours(1), Type = EventType.Appointment, Subject = "Test2" },
                new Event { Start = saturday, End = saturday.AddHours(1), Type = EventType.Appointment, Subject = "Test3" },
            });
            ImportantEMails = new BindableCollection<MailItem> { new MailItem { Start = sunday, Subject = "Test4" } };
        }

        async private void RefreshData()
        {
            if (!IsRefreshing && !PauseRefresh)
            {
                IsRefreshing = true;

                await Task.Run(() =>
                 {
                     var oNamespace = OutlookUtils.GetOutlookNameSpace();
                     DateTime weekStart;
                     DateTime weekEnd;

                     if (DateTime.Now.DayOfWeek >= DayOfWeek.Thursday && DateTime.Now.DayOfWeek <= DayOfWeek.Saturday)
                         weekStart = DayOfWeek.Wednesday.GetThisWeekday();
                     else
                         weekStart = DayOfWeek.Sunday.GetThisWeekday();

                     weekEnd = weekStart.AddDays(6);

                     var apptItems = OutlookUtils.GetCalendarItems(oNamespace, weekStart, weekEnd);
                     var taskItems = OutlookUtils.GetTasksItems(oNamespace, weekStart, weekEnd);
                     Week = new Week(weekStart, weekEnd, apptItems.Concat(taskItems).OrderBy(item => item.Start).ToList());
                 });

                await Task.Run(() =>
                {
                    var oNamespace = OutlookUtils.GetOutlookNameSpace();
                    var flaggedMailItems = OutlookUtils.GetMailItems(oNamespace, true);
                    ImportantEMails = new BindableCollection<MailItem>(flaggedMailItems.OrderByDescending(item => item.Start));
                });

                IsRefreshing = false;
            }
        }
    }
}