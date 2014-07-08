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

        private void SectorCompleteTest_Click(object sender, RoutedEventArgs e)
        {
            window.LapTimer.SectorComplete();
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
    }
}
