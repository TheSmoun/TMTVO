using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
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
    public partial class Controls : Window
    {
        public CameraModule CameraModule { get; set; }
        public DriverModule DriverModule { get; set; }

        private Controller.TMTVO tmtvo;
        private F1TVOverlay f1Window;
        private Timer t;
        private SessionTimer.SessionMode sessionTimerMode = TMTVO.Widget.F1.SessionTimer.SessionMode.TimeMode;
        private int driverCount = 0;
        private Timer statusBarTimer;
        private API api;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private DispatcherTimer updateTimer;
        private bool autoCommit;
        private bool cameraUpdate;
        private bool driverUpdate;

        public Controls(API api, F1TVOverlay window, Controller.TMTVO tmtvo)
        {
            this.api = api;
            this.autoCommit = true;
            this.driverUpdate = true;
            this.cameraUpdate = true;

            this.tmtvo = tmtvo;
            this.f1Window = window;
            InitializeComponent();

            statusBarTimer = new Timer(1000);
            statusBarTimer.Elapsed += updateStatusBar;
            statusBarTimer.Start();
        }

        public void UpdateWindow()
        {
            /*if (driverCount == DriverModule.Drivers.Count)
                return;

            int selIdx = DriversLeft.SelectedIndex;
            DriversLeft.Items.Clear();
            for (int carIdx = 0; carIdx < DriverModule.Drivers.Count; carIdx++)
            {
                DriversLeft.Items.Add(DriverModule.Drivers.Find(d => d.CarIndex == carIdx));
            }

            driverCount = DriverModule.Drivers.Count;*/
        }

        public void UpdateLaunchButton(API api)
        {
            if (!api.IsConnected && f1Window.Visibility == Visibility.Visible)
            {
                StartStopButton_Click(null, null);
                StartStop.IsEnabled = false;
            }
            else if (api.IsConnected)
            {
                StartStop.IsEnabled = true;
            }
            else
            {
                StartStop.IsEnabled = false;
                
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tmtvo.Api.Run = false;
            f1Window.Close();
            Environment.Exit(0);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.Visibility != Visibility.Visible)
            {
                StartStop.Content = "Stop";
                tmtvo.Api.Run = true;
                tmtvo.Api.Start();
                f1Window.Show();
                f1Window.Visibility = Visibility.Visible;
                t = new Timer(50);
                t.Elapsed += ShowTabs;
                t.Start();
                // TODO Load Theme
                TabGrid.Visibility = Visibility.Visible;

                ThemeSelector.IsEnabled = false;
            }
            else
            {
                StartStop.Content = "Start";
                tmtvo.Api.Run = false;
                f1Window.Visibility = Visibility.Hidden;

                foreach (object o in f1Window.MyCanvas.Children)
                    if (o is IWidget && ((IWidget)o).Active)
                        ((IWidget)o).FadeOut();

                tmtvo.Api.Stop();
                TabGrid.Visibility = Visibility.Hidden;

                ThemeSelector.IsEnabled = true;
            }
        }

        private void ShowTabs(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SessionTimerModule sTModule = (SessionTimerModule)tmtvo.Api.FindModule("SessionTimer");
                if (sTModule.SessionType == SessionType.LapRace || sTModule.SessionType == SessionType.TimeRace)
                {
                    Race.Visibility = Visibility.Visible;
                    Practice.Visibility = Visibility.Hidden;
                }
                else
                {
                    Race.Visibility = Visibility.Hidden;
                    Practice.Visibility = Visibility.Visible;
                }
            }));

            t.Stop();
        }

        private void SessionTimer_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.SessionTimer.Active)
                f1Window.SessionTimer.FadeOut();
            else
                f1Window.SessionTimer.FadeIn(sessionTimerMode);
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
            if (f1Window.LiveTiming.Active)
                f1Window.LiveTiming.FadeOut();
            else
                f1Window.LiveTiming.FadeIn();
        }

        private void TimingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            f1Window.LiveTiming.ChangeMode((LiveTimingItemMode)((ComboBox)sender).SelectedIndex);
        }

        private void ShowHideLeftTimer_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.LapTimerLeft.Active)
            {
                //ShowHideLeftTimer.Content = "Show LapTimer L";
                f1Window.LapTimerLeft.FadeOut();
                //DriversLeft.IsEnabled = true;
            }
            else
            {
                int carIdx = /*DriversLeft.SelectedIndex;*/ CameraModule.FollowedDriver;
                if (carIdx == -1)
                    return;

                LiveStandingsItem driver = ((LiveStandingsModule)tmtvo.Api.FindModule("LiveStandings")).FindDriver(carIdx);
                if (driver == null)
                    return;

                //ShowHideLeftTimer.Content = "Hide LapTimer L";
                f1Window.LapTimerLeft.FadeIn(driver);
                //DriversLeft.IsEnabled = false;
            }
        }

        private void RaceBarToggle_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.RaceBar.Active)
                f1Window.RaceBar.FadeOut();
            else
                f1Window.RaceBar.FadeIn();
        }

        private void RaceBartModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            f1Window.RaceBar.Mode = (RaceBar.RaceBarMode)((ComboBox)sender).SelectedIndex;
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
            f1Window.RaceBar.Live = true;
        }

        private void RaceBarLive_Unchecked(object sender, RoutedEventArgs e)
        {
            f1Window.RaceBar.Live = false;
        }

        private void ResultsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (f1Window.SessionTimer.Module.SessionType)
            {
                case SessionType.LapRace:
                case SessionType.TimeRace:
                    f1Window.ResultsWidget.Show(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Race);
                    break;
                default:
                    f1Window.ResultsWidget.Show(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Practice);
                    break;
            }
        }

        private void TimingPrevPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTiming.PrevPage();
        }

        private void TimingNextPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTiming.NextPage();
        }

        private void TimingLeaderPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTiming.LeaderPage();
        }

        private void WeatherToggle_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.WeatherWidget.Active)
                f1Window.WeatherWidget.FadeOut();
            else
                f1Window.WeatherWidget.FadeIn();
        }

        private void LiveTimingLeaderOnly_Checked(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTiming.FadeOutPage();
        }

        private void LiveTimingLeaderOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTiming.FadeInPage();
        }

        public void Reset()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                cameraSelectComboBox.Items.Clear();
                driverSelect.Items.Clear();
            }));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private void controlsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            updateTimer.Tick += updateControls;
            updateTimer.Start();
            cameraSelectComboBox.Items.Clear();
            driverSelect.Items.Clear();
            updateControls(new object(), new EventArgs());
        }

        private void updateControls(object sender, EventArgs e)
        {
            if (!api.IsConnected || CameraModule == null || DriverModule == null)
                return;

            bool oldAutoCommit = autoCommit;
            autoCommit = false;

            ComboBoxItem cboxitem;
            if (cameraUpdate)
            {
                if (CameraModule.Cameras.Count > 0)
                {
                    cameraSelectComboBox.Items.Clear();
                    foreach (Camera cam in CameraModule.Cameras)
                    {
                        cboxitem = new ComboBoxItem();
                        cboxitem.Content = cam.Name;
                        cboxitem.Tag = cam.Id;
                        cameraSelectComboBox.Items.Add(cboxitem);
                        if (cam.Id == CameraModule.CurrentCamera)
                            cameraSelectComboBox.SelectedItem = cboxitem;
                    }
                }
            }

            if (driverUpdate)
            {
                driverSelect.Items.Clear();

                cboxitem = new ComboBoxItem();
                cboxitem.Content = "Most exiting";
                cboxitem.Tag = -1;
                driverSelect.Items.Add(cboxitem);

                cboxitem = new ComboBoxItem();
                cboxitem.Content = "Leader";
                cboxitem.Tag = -2;
                driverSelect.Items.Add(cboxitem);

                cboxitem = new ComboBoxItem();
                cboxitem.Content = "Crashes";
                cboxitem.Tag = -3;
                driverSelect.Items.Add(cboxitem);

                IEnumerable<Driver> dQuery = DriverModule.Drivers.OrderBy(s => s.NumberPlateInt);

                foreach (Driver driver in dQuery)
                {
                    cboxitem = new ComboBoxItem();
                    cboxitem.Content = driver.Car.CarNumber + " " + driver.FullName;
                    cboxitem.Tag = padCarNum(driver.Car.CarNumber);
                    driverSelect.Items.Add(cboxitem);
                    if (driver.CarIndex == CameraModule.FollowedDriver)
                        driverSelect.SelectedItem = cboxitem;
                }
            }

            if ((api != null) && api.IsConnected && (api.GetData("ReplayPlaySpeed") != null))
            {
                int playspeed = (int)api.GetData("ReplayPlaySpeed");
                if (playspeed != 1)
                    playButton.Content = "4";
                else
                    playButton.Content = ";";
            }

            autoCommit = oldAutoCommit;
        }

        public static int padCarNum(string input)
        {
            int num = Int32.Parse(input);
            int zero = input.Length - num.ToString().Length;

            int retVal = num;
            int numPlace = 1;
            if (num > 99)
                numPlace = 3;
            else if (num > 9)
                numPlace = 2;
            if (zero > 0)
            {
                numPlace += zero;
                retVal = num + 1000 * numPlace;
            }

            return retVal;
        }

        private void addBookmark_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cameraSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autoCommit)
                commit();
        }

        private void commit()
        {
            if (driverSelect.SelectedItem == null || cameraSelectComboBox.SelectedItem == null)
                return;

            int driver = Convert.ToInt32(driverSelect.SelectedValue);
            int camera = Convert.ToInt32(cameraSelectComboBox.SelectedValue);

            api.SwitchCamera(driver, camera);
        }

        private void commitButton_Click(object sender, RoutedEventArgs e)
        {
            commit();
        }

        private void autoCommitButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;
            if (autoCommit)
            {
                btn.Content = "Auto apply";
                autoCommit = false;
            }
            else
            {
                btn.Content = "Manual apply";
                autoCommit = true;
            }
        }

        private void addBookmark_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void PlaySpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void instantReplay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void liveButton_Click(object sender, RoutedEventArgs e)
        {
            api.ReplaySearch(ReplaySearchModeTypes.ToEnd, 0);
            api.Play();
            // TODO Trigger
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (api.IsConnected)
            {
                int playspeed = (int)api.GetData("ReplayPlaySpeed");
                if (playspeed != 1)
                {
                    api.Play();
                    playButton.Content = "4";
                }
                else
                {
                    api.Pause();
                    playButton.Content = ";";
                }
            }
        }

        private void prevDriver_Click(object sender, RoutedEventArgs e)
        {
            driver(-1);
        }

        private void nextDriver_Click(object sender, RoutedEventArgs e)
        {
            driver(1);
        }

        private void driver(int delta)
        {
            LiveStandingsModule lsm = Controller.TMTVO.Instance.Api.FindModule("LiveStandings") as LiveStandingsModule;

            int pos = lsm.FindDriver(CameraModule.FollowedDriver).Position + delta;
            string nextPlate = "";

            if (pos < 1)
                nextPlate = lsm.FindDriverByPos(DriverModule.Drivers.Count).Driver.Car.CarNumber;
            else if (pos > lsm.Items.Count)
                nextPlate = lsm.Leader.Driver.Car.CarNumber;
            else
                nextPlate = lsm.FindDriverByPos(pos).Driver.Car.CarNumber;

            if (autoCommit)
                api.SwitchCamera(padCarNum(nextPlate), Convert.ToInt32(cameraSelectComboBox.SelectedValue));
        }

        private void uiCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //if (uiCheckBox.IsChecked == false)
            //    SharedData.showSimUi = true;
            //else
            //    SharedData.showSimUi = false;
        }

        private void controlsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void driverSelect_DropDownOpened(object sender, EventArgs e)
        {
            driverUpdate = false;
        }

        private void driverSelect_DropDownClosed(object sender, EventArgs e)
        {
            driverUpdate = true;
        }

        private void cameraSelectComboBox_DropDownOpened(object sender, EventArgs e)
        {
            cameraUpdate = false;
        }

        private void cameraSelectComboBox_DropDownClosed(object sender, EventArgs e)
        {
            cameraUpdate = true;
        }
    }
}
