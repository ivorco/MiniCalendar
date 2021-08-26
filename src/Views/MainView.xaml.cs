using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MiniCalendar.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            // Load last size and position
            if (Properties.Settings.Default.LastPosition != System.Drawing.Point.Empty &&
    Properties.Settings.Default.LastSize != System.Drawing.Size.Empty)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;

                Left = Properties.Settings.Default.LastPosition.X;
                Top = Properties.Settings.Default.LastPosition.Y;
                Width = Properties.Settings.Default.LastSize.Width;
                Height = Properties.Settings.Default.LastSize.Height;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Set and save last size and position
            Properties.Settings.Default.LastPosition = new System.Drawing.Point((int)Left, (int)Top);
            Properties.Settings.Default.LastSize = new System.Drawing.Size((int)Width, (int)Height);
            Properties.Settings.Default.Save();

            // Wait until finished refreshing
            WaitRefereshing();
        }

        private void WaitRefereshing()
        {
            while (((ViewModels.MainViewModel)DataContext).IsRefreshing)
                System.Threading.Thread.Sleep(100);
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

        private void RotationStoryboardCompleted(object sender, EventArgs e)
        {
            var viewModel = (ViewModels.MainViewModel)DataContext;
            var storyboard = (Storyboard)((ClockGroup)sender).Timeline;

            if (viewModel.IsRefreshing)
                storyboard.Begin();
        }

        private void Event_Click(object sender, RoutedEventArgs e)
        {
            WaitRefereshing();
            ((ViewModels.MainViewModel)DataContext).PauseRefresh = true;

            var eventId = ((Data.Event)((Button)sender).DataContext).ID;
            OutlookUtils.DisplayEvent(eventId);

            ((ViewModels.MainViewModel)DataContext).PauseRefresh = false;
            ((ViewModels.MainViewModel)DataContext).Refresh();
        }

        private void SetDropHighlightVisibility(object sender, Visibility visibility)
        {
            var childDropHighlight = ((Panel)sender).Children.OfType<Border>().First(child => child.Name == "DropHighlight");
            childDropHighlight.Visibility = visibility;
        }

        private void Day_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                SetDropHighlightVisibility(sender, Visibility.Visible);
                e.Handled = true;
            }
        }

        private void Day_DragLeave(object sender, DragEventArgs e)
        {
            SetDropHighlightVisibility(sender, Visibility.Hidden);
            e.Handled = true;
        }

        private void Day_Drop(object sender, DragEventArgs e)
        {
            SetDropHighlightVisibility(sender, Visibility.Hidden);
        }

        private void DaySide_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void DaySide_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void TaskDaySide_Drop(object sender, DragEventArgs e)
        {
            DayDropAdd(sender, e, Data.EventType.Task);
        }

        private void AppointmentDaySide_Drop(object sender, DragEventArgs e)
        {
            DayDropAdd(sender, e, Data.EventType.Appointment);
        }

        private void DayDropAdd(object sender, DragEventArgs e, Data.EventType eventType)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Handled = true;

                var dropDate = ((Data.Day)((Border)sender)?.DataContext).Date;
                var dropData = e.Data.GetData(DataFormats.UnicodeText).ToString();
                if (eventType == Data.EventType.Appointment)
                    OutlookUtils.AddAppointment(dropDate, dropData);
                else if (eventType == Data.EventType.Task)
                    OutlookUtils.AddTask(dropDate, dropData);
            }

            // TODO: How to make it nicer (maybe search up the tree of controls for the border)
            SetDropHighlightVisibility((((sender as Border).Parent as Grid).Parent as Border).Parent, Visibility.Hidden);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
