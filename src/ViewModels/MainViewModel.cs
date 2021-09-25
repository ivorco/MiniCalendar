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

        // Nice to have
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
        // TODO: minical -> focus point

        public MainViewModel()
        {
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

        async public void RefreshData()
        {
            if (!IsRefreshing && !PauseRefresh)
            {
                IsRefreshing = true;

                await Task.Run(() =>
                 {
                     var oNamespace = OutlookUtils.GetOutlookNameSpace();
                     DateTime weekStart;
                     DateTime weekEnd;

                     if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
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