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
        // TODO: Properly run updates on thread and timers
        // TODO: Click to add new appointment or task
        // TODO: Drag text to add
        // TODO: Open items in outlook
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

            var timerCurrentTime = new Timer(500);
            timerCurrentTime.Elapsed += (s, e) =>
            {
                CurrentTime = DateTime.Now;
                SnoozeTime = SnoozeTime;
                NextEvents = new BindableCollection<Event>(Week.AllEvents().Where(wevent => wevent.Start - CurrentTime < TimeSpan.FromMinutes(30) && wevent.Start - CurrentTime > TimeSpan.Zero));
            };
            timerCurrentTime.Start();
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

                IsRefreshing = false;
            }
        }
    }
}