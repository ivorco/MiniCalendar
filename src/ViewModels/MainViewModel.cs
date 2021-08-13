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
        // TODO: Highlight upcoming appoinments
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

        private Week week = new Week(DateTime.Now, DateTime.Now.AddDays(-1));
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