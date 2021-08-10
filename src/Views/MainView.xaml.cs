using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MiniCalendar.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
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
    }
}
