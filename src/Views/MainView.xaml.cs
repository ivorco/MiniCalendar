﻿using System;
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
    }
}
