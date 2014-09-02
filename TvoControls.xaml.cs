﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.Widget;
using TMTVO.Widget.F1;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für Controls.xaml
    /// </summary>
    public partial class TvoControls : Window
    {
        private TMTVO.Controller.TMTVO tmtvo;
        private F1TVOverlay window;
        private Timer t;
        private SessionTimer.SessionMode sessionTimerMode = Widget.F1.SessionTimer.SessionMode.TimeMode;
        private int driverCount = 0;
        private Timer statusBarTimer;

        public TvoControls(F1TVOverlay window, TMTVO.Controller.TMTVO tmtvo)
        {
            this.tmtvo = tmtvo;
            this.window = window;
            InitializeComponent();

            statusBarTimer = new Timer(1000);
            statusBarTimer.Elapsed += updateStatusBar;
            statusBarTimer.Start();
        }

        public void UpdateWindow()
        {
            DriverModule dM = ((DriverModule)tmtvo.Api.FindModule("DriverModule"));
            if (driverCount == dM.Drivers.Count)
                return;

            int selIdx = DriversLeft.SelectedIndex;
            DriversLeft.Items.Clear();
            for (int carIdx = 0; carIdx < dM.Drivers.Count; carIdx++)
            {
                DriversLeft.Items.Add(dM.Drivers.Find(d => d.CarIndex == carIdx));
            }

            driverCount = dM.Drivers.Count;
        }

        public void UpdateLaunchButton(API api)
        {
            if (!api.IsConnected && window.Visibility == Visibility.Visible)
            {
                StartStopButton_Click(null, null);
                StartStopButton.IsEnabled = false;
            }
            else if (api.IsConnected)
            {
                StartStopButton.IsEnabled = true;

                if (!tmtvo.iRControls.IsVisible)
                    tmtvo.iRControls.Show();
            }
            else
            {
                StartStopButton.IsEnabled = false;

                if (tmtvo.iRControls.IsVisible)
                    tmtvo.iRControls.Hide();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmtvo.Api.Run = false;
            window.Close();
            Environment.Exit(0);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (window.Visibility != Visibility.Visible)
            {
                Running.Fill = new SolidColorBrush(Colors.Green);
                StartStopButton.Content = "Stop";
                tmtvo.Api.Run = true;
                tmtvo.Api.Start();
                window.Show();
                window.Visibility = Visibility.Visible;
                t = new Timer(50);
                t.Elapsed += ShowGrid;
                t.Start();

                tmtvo.iRControls.Visibility = Visibility.Visible;
            }
            else
            {
                Running.Fill = new SolidColorBrush(Colors.Red);
                StartStopButton.Content = "Start";
                tmtvo.Api.Run = false;
                window.Visibility = Visibility.Hidden;
                InnerGrid.Visibility = Visibility.Hidden;

                foreach (object o in window.MyCanvas.Children)
                    if (o is IWidget && ((IWidget)o).Active)
                        ((IWidget)o).FadeOut();

                tmtvo.Api.Stop();

                tmtvo.iRControls.Visibility = Visibility.Hidden;
            }
        }

        private void ShowGrid(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SessionTimerModule sTModule = (SessionTimerModule)tmtvo.Api.FindModule("SessionTimer");
                if (sTModule.SessionType == SessionType.LapRace || sTModule.SessionType == SessionType.TimeRace)
                {
                    RaceButtons.Visibility = Visibility.Hidden; // TODO Change
                    NormalButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    RaceButtons.Visibility = Visibility.Hidden;
                    NormalButtons.Visibility = Visibility.Visible;
                }

                InnerGrid.Visibility = Visibility.Visible;
            }));

            t.Stop();
        }

        private void SessionTimer_Click(object sender, RoutedEventArgs e)
        {
            if (window.SessionTimer.Active)
                window.SessionTimer.FadeOut();
            else
                window.SessionTimer.FadeIn(sessionTimerMode);
        }

        private void SessionTimerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    sessionTimerMode = Widget.F1.SessionTimer.SessionMode.TimeMode;
                    break;
                case 1:
                    sessionTimerMode = Widget.F1.SessionTimer.SessionMode.LapMode;
                    break;
                default:
                    sessionTimerMode = Widget.F1.SessionTimer.SessionMode.TimeMode;
                    break;
            }
        }

        private void TeamRadioEnabled_Checked(object sender, RoutedEventArgs e)
        {
            ((TeamRadioModule)tmtvo.Api.FindModule("TeamRadio")).CanShowTeamRadio = true;
        }

        private void TeamRadioEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            ((TeamRadioModule)tmtvo.Api.FindModule("TeamRadio")).CanShowTeamRadio = false;
        }

        private void ShowHideTiming_Click(object sender, RoutedEventArgs e)
        {
            if (window.LiveTiming.Active)
                window.LiveTiming.FadeOut();
            else
                window.LiveTiming.FadeIn();
        }

        private void TimingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            window.LiveTiming.ChangeMode((LiveTimingItemMode)((ComboBox)sender).SelectedIndex);
        }

        private void ShowHideLeftTimer_Click(object sender, RoutedEventArgs e)
        {
            if (window.LapTimerLeft.Active)
            {
                ShowHideLeftTimer.Content = "Show LapTimer L";
                window.LapTimerLeft.FadeOut();
                DriversLeft.IsEnabled = true;
            }
            else
            {
                int carIdx = DriversLeft.SelectedIndex;
                if (carIdx == -1)
                    return;

                LiveStandingsItem driver = ((LiveStandingsModule)tmtvo.Api.FindModule("LiveStandings")).FindDriver(carIdx);
                if (driver == null)
                    return;

                ShowHideLeftTimer.Content = "Hide LapTimer L";
                window.LapTimerLeft.FadeIn(driver);
                DriversLeft.IsEnabled = false;
            }
        }

        private void RaceBarToggle_Click(object sender, RoutedEventArgs e)
        {
            if (window.RaceBar.Active)
                window.RaceBar.FadeOut();
            else
                window.RaceBar.FadeIn();
        }

        private void RaceBartModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            window.RaceBar.Mode = (RaceBar.RaceBarMode)((ComboBox)sender).SelectedIndex;
        }

        private void updateStatusBar(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (tmtvo.Api.IsConnected)
                    StatusText.Content = "iRacing connected.";
                else
                    StatusText.Content = "iRacing not connected.";
            }));
        }

        private void RaceBarLive_Checked(object sender, RoutedEventArgs e)
        {
            window.RaceBar.Live = true;
        }

        private void RaceBarLive_Unchecked(object sender, RoutedEventArgs e)
        {
            window.RaceBar.Live = false;
        }

        private void ResultsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (window.SessionTimer.Module.SessionType)
            {
                case SessionType.LapRace:
                case SessionType.TimeRace:
                    window.ResultsWidget.Show(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Race);
                    break;
                default:
                    window.ResultsWidget.Show(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Practice);
                    break;
            }
        }

        private void TimingPrevPage_Click(object sender, RoutedEventArgs e)
        {
            window.LiveTiming.PrevPage();
        }

        private void TimingNextPage_Click(object sender, RoutedEventArgs e)
        {
            window.LiveTiming.NextPage();
        }

        private void TimingLeaderPage_Click(object sender, RoutedEventArgs e)
        {
            window.LiveTiming.LeaderPage();
        }

        private void WeatherToggle_Click(object sender, RoutedEventArgs e)
        {
            if (window.WeatherWidget.Active)
                window.WeatherWidget.FadeOut();
            else
                window.WeatherWidget.FadeIn();
        }

        private void LiveTimingLeaderOnly_Checked(object sender, RoutedEventArgs e)
        {
            window.LiveTiming.FadeOutPage();
        }

        private void LiveTimingLeaderOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            window.LiveTiming.FadeInPage();
        }
    }
}
