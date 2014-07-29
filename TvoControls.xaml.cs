using System;
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
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.Widget;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für Controls.xaml
    /// </summary>
    public partial class TvoControls : Window
    {
        private TMTVO.Controller.TMTVO tmtvo;
        private MainWindow window;
        private Timer t;
        private SessionTimer.SessionMode sessionTimerMode = Widget.SessionTimer.SessionMode.TimeMode;
        private int driverCount = 0;

        public TvoControls(MainWindow window, TMTVO.Controller.TMTVO tmtvo)
        {
            this.tmtvo = tmtvo;
            this.window = window;
            InitializeComponent();
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
            }
        }

        private void ShowGrid(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SessionTimerModule sTModule = (SessionTimerModule)tmtvo.Api.FindModule("SessionTimer");
                if (sTModule.SessionType == SessionType.LapRace || sTModule.SessionType == SessionType.TimeRace)
                {
                    RaceButtons.Visibility = Visibility.Visible;
                    NormalButtons.Visibility = Visibility.Hidden;
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
                    sessionTimerMode = Widget.SessionTimer.SessionMode.TimeMode;
                    break;
                case 1:
                    sessionTimerMode = Widget.SessionTimer.SessionMode.LapMode;
                    break;
                default:
                    sessionTimerMode = Widget.SessionTimer.SessionMode.TimeMode;
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
            {
                ShowHideTiming.Content = "Show Live Timing";
                window.LiveTiming.FadeOut();
            }
            else
            {
                ShowHideTiming.Content = "Hide Live Timing";
                window.LiveTiming.FadeIn();
            }
        }

        private void TimingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            window.LiveTiming.Mode = (LiveTimingItemMode)((ComboBox)sender).SelectedIndex;
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
    }
}
