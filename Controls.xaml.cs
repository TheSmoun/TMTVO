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
using TMTVO.Widget;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für Controls.xaml
    /// </summary>
    public partial class Controls : Window
    {
        private MainWindow window;
        private Timer t;

        public Controls(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DataManager.RunApi = false;
            window.LapTimer.Thread.Interrupt();
            window.Close();
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataManager.RunOverlay)
            {
                Running.Fill = new SolidColorBrush(Colors.Green);
                StartStopButton.Content = "Stop";
                DataManager.RunOverlay = true;
                t = new Timer(50);
                t.Elapsed += ShowGrid;
                t.Start();
            }
            else
            {
                Running.Fill = new SolidColorBrush(Colors.Red);
                StartStopButton.Content = "Start";
                DataManager.RunOverlay = false;
                InnerGrid.Visibility = Visibility.Hidden;

                foreach (object o in window.MyCanvas.Children)
                    if (o is IWidget && ((IWidget)o).Active)
                        ((IWidget)o).FadeOut();
            }
        }

        private void ShowGrid(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (DataManager.Sessions.CurrentSession.SessionType == SessionType.Race)
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
            {
                Widget.SessionTimer.SessionType type;
                Widget.SessionTimer.SessionMode mode = Widget.SessionTimer.SessionMode.TimeMode;

                switch (DataManager.Sessions.CurrentSession.SessionType)
                {
                    case SessionType.OfflineTesting:
                    case SessionType.Practice:
                        type = Widget.SessionTimer.SessionType.OpenPractice;
                        break;
                    case SessionType.Qualifying:
                        type = Widget.SessionTimer.SessionType.Qualify;
                        break;
                    case SessionType.Race:
                        if (DataManager.Sessions.CurrentSession.SessionLaps != int.MaxValue)
                        {
                            mode = Widget.SessionTimer.SessionMode.LapMode;
                            type = Widget.SessionTimer.SessionType.LapRace;
                        }
                        else
                        {
                            type = Widget.SessionTimer.SessionType.TimeRace;
                        }
                        break;
                    case SessionType.WarmUp:
                        type = Widget.SessionTimer.SessionType.WarmUp;
                        break;
                    default:
                        type = Widget.SessionTimer.SessionType.TimeTrial;
                        break;
                }

                window.SessionTimer.FadeIn(mode, type);
            }
        }

        private void SectorCompleteTest_Click(object sender, RoutedEventArgs e)
        {
            window.LapTimer.SectorComplete();
        }

        ResultItem item = new ResultItem();

        private void ToggleLapTimerLeft_Click(object sender, RoutedEventArgs e)
        {
            if (window.LapTimer.Active)
            {
                window.LapTimer.FadeOut();
            }
            else
            {
                Driver testDriver = new Driver();
                testDriver.FullName = "Simon Grossmann";
                testDriver.Car = new Car();
                testDriver.Car.CarNumber = "46";
                testDriver.CarIndex = 0;
                item.Position = 1;
                item.Driver = testDriver;

                window.LapTimer.FadeIn(item);
            }
        }

        private void LapCompleteTest_Click(object sender, RoutedEventArgs e)
        {
            window.LapTimer.LapComplete();
        }

        private void CrossedLine_Click(object sender, RoutedEventArgs e)
        {
            item.CrossedLine();
        }
    }
}
