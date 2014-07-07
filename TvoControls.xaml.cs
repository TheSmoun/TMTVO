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

        public TvoControls(MainWindow window, TMTVO.Controller.TMTVO tmtvo)
        {
            this.tmtvo = tmtvo;
            this.window = window;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmtvo.Api.Run = false;
            if (window.LapTimer.Thread != null)
                window.LapTimer.Thread.Interrupt();
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
                if (tmtvo.sessionTimerModule.SessionType == SessionType.LapRace || tmtvo.sessionTimerModule.SessionType == SessionType.TimeRace)
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

                switch (tmtvo.sessionTimerModule.SessionType)
                {
                    case SessionType.OfflineTesting:
                    case SessionType.Practice:
                        type = Widget.SessionTimer.SessionType.OpenPractice;
                        break;
                    case SessionType.Qualifying:
                        type = Widget.SessionTimer.SessionType.Qualify;
                        break;
                    case SessionType.LapRace:
                        mode = Widget.SessionTimer.SessionMode.LapMode;
                        type = Widget.SessionTimer.SessionType.LapRace;
                        break;
                    case SessionType.TimeRace:
                        type = Widget.SessionTimer.SessionType.TimeRace;
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
    }
}
