using MiniCalendar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MiniCalendar
{
    // TODO: Refresh every x time, with force refresh button
    // TODO: Dark mode
    // TODO: Highlight upcoming appoinments and current day

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var items = GetWeeklyCalendarItems();
            //MessageBox.Show(string.Join(Environment.NewLine, items.OrderBy(item => item.Start).Select(item => item.Subject)));

            var week = new Week(items.ToList());

            foreach (var day in week)
                itemsControl.Items.Add(day);
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
