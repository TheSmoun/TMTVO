using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TMTVO;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.Widget;
using TMTVO_Api.ThemeApi;

namespace TMTVO_F1Theme
{
	/// <summary>
	/// Interaktionslogik für ControlTabs.xaml
	/// </summary>
	public partial class F1ControlTabs : UserControl, IControlPanel
	{
        public IThemeWindow ThemeWindow { get; private set; }
        public CameraModule CameraModule { get; set; }
        public DriverModule DriverModule { get; set; }

        internal F1TVOverlay f1Window;
        private API api;
        private DispatcherTimer updateTimer;
        private bool autoCommit;
        private bool cameraUpdate;
        private bool driverUpdate;

        private bool canUpdateDO1;
        private bool canUpdateDO2;
        private bool canUpdateFp;

        public F1ControlTabs(IThemeWindow window, API api)
        {
            ThemeWindow = window;
            f1Window = window as F1TVOverlay;

            this.api = api;
            this.autoCommit = true;
            this.driverUpdate = true;
            this.cameraUpdate = true;
            this.canUpdateDO1 = true;
            this.canUpdateDO2 = true;
            this.canUpdateFp = true;

            InitializeComponent();

            CameraModule = api.FindModule("CameraModule") as CameraModule;
            DriverModule = api.FindModule("DriverModule") as DriverModule;
        }

        private void ShowTabs(object sender, EventArgs e)
        {
            SessionTimerModule sTModule = (SessionTimerModule)api.FindModule("SessionTimer");
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
        }

        private void SessionTimer_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.SessionTimer.Active)
                f1Window.SessionTimerFadeOut();
            else
                f1Window.SessionTimerFadeIn();
        }

        private void TeamRadioEnabled_Checked(object sender, RoutedEventArgs e)
        {
            ((TeamRadioModule)api.FindModule("TeamRadio")).CanShowTeamRadio = true;
        }

        private void TeamRadioEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            ((TeamRadioModule)api.FindModule("TeamRadio")).CanShowTeamRadio = false;
        }

        private void ShowHideTiming_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.LiveTimingWidget.Active)
                f1Window.LiveTimingWidgetFadeOut();
            else
                f1Window.LiveTimingWidgetFadeIn();
        }

        private void TimingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (f1Window != null)
                f1Window.LiveTimingWidget.ChangeMode((LiveTimingItemMode)((ComboBox)sender).SelectedIndex);
        }

        private void ShowHideLeftTimer_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.LapTimerLeft.Active)
                f1Window.LapTimerLeftFadeOut();
            else
            {
                int carIdx = CameraModule.FollowedDriver;
                if (carIdx == -1)
                    return;

                LiveStandingsItem driver = ((LiveStandingsModule)api.FindModule("LiveStandings")).FindDriver(carIdx);
                if (driver == null)
                    return;

                f1Window.LapTimerLeftFadeIn(driver);
            }
        }

        private void RaceBarToggle_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.RaceBar.Active)
                f1Window.RaceBarFadeOut();
            else
                f1Window.RaceBarFadeIn();
        }

        private void RaceBartModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (f1Window != null)
                f1Window.RaceBar.Mode = (RaceBar.RaceBarMode)((ComboBox)sender).SelectedIndex;
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
            switch ((api.FindModule("SessionTimer") as SessionTimerModule).SessionType)
            {
                case SessionType.LapRace:
                case SessionType.TimeRace:
                    f1Window.ResultsFadeIn(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Race);
                    break;
                default:
                    f1Window.ResultsFadeIn(ResultsWidget.MS_PER_PAGE, ResultsWidget.ResultsMode.Practice);
                    break;
            }
        }

        private void TimingPrevPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTimingWidget.PrevPage();
        }

        private void TimingNextPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTimingWidget.NextPage();
        }

        private void TimingLeaderPage_Click(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTimingWidget.LeaderPage();
        }

        private void WeatherToggle_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.WeatherWidget.Active)
                f1Window.WeatherFadeOut();
            else
                f1Window.WeatherFadeIn();
        }

        private void LiveTimingLeaderOnly_Checked(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTimingWidget.FadeOutPage();
        }

        private void LiveTimingLeaderOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            f1Window.LiveTimingWidget.FadeInPage();
        }

        public void Reset()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                cameraSelectComboBox.Items.Clear();
                driverSelect.Items.Clear();
            }));
        }

        /*
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

         */

        private void controlsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            updateTimer.Tick += updateControls;
            updateTimer.Tick += api.Connect;
            updateTimer.Tick += api.UpdateControls;
            updateTimer.Tick += ShowTabs;
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

                IEnumerable<Driver> dQuery = DriverModule.OrderDriversByNumberPlate();

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

            SessionTimerModule stm = api.FindModule("SessionTimer") as SessionTimerModule;
            if (stm.SessionType == SessionType.LapRace || stm.SessionType == SessionType.TimeRace)
            {
                QualiTimeOnly.IsEnabled = true;
                QualiTimeWithGap.IsEnabled = true;
                Improvements.IsEnabled = true;
            }
            else
            {
                QualiTimeOnly.IsEnabled = false;
                QualiTimeWithGap.IsEnabled = false;
                Improvements.IsEnabled = false;
            }

            if (canUpdateDO1)
            {
                int tag = int.Parse(DriverOverviewDriver1.SelectedValue.ToString());

                DriverOverviewDriver1.Items.Clear();

                ComboBoxItem item = new ComboBoxItem();
                item.Tag = -1;
                item.Content = "No Driver";

                DriverOverviewDriver1.Items.Add(item);
                if ((int)item.Tag == tag)
                    DriverOverviewDriver1.SelectedItem = item;

                IEnumerable<Driver> dQuery = DriverModule.OrderDriversByNumberPlate();
                foreach (Driver driver in dQuery)
                {
                    item = new ComboBoxItem();
                    item.Content = driver.Car.CarNumber + " " + driver.FullName;
                    item.Tag = driver.CarIndex;
                    DriverOverviewDriver1.Items.Add(item);
                    if ((int)item.Tag == tag)
                        DriverOverviewDriver1.SelectedItem = item;
                }
            }

            if (canUpdateDO2)
            {
                int tag = int.Parse(DriverOverviewDriver2.SelectedValue.ToString());

                DriverOverviewDriver2.Items.Clear();

                ComboBoxItem item = new ComboBoxItem();
                item.Tag = -1;
                item.Content = "No Driver";

                DriverOverviewDriver2.Items.Add(item);
                if ((int)item.Tag == tag)
                    DriverOverviewDriver2.SelectedItem = item;

                IEnumerable<Driver> dQuery = DriverModule.OrderDriversByNumberPlate();
                foreach (Driver driver in dQuery)
                {
                    item = new ComboBoxItem();
                    item.Content = driver.Car.CarNumber + " " + driver.FullName;
                    item.Tag = driver.CarIndex;
                    DriverOverviewDriver2.Items.Add(item);
                    if ((int)item.Tag == tag)
                        DriverOverviewDriver2.SelectedItem = item;
                }
            }

            if (canUpdateFp)
            {
                int tag = 1;
                if (FirstPos.SelectedValue != null)
                    tag = int.Parse(FirstPos.SelectedValue.ToString());

                FirstPos.Items.Clear();

                ComboBoxItem item;
                for (int i = 1; i <= DriverModule.Drivers.Count; i++)
                {
                    item = new ComboBoxItem();
                    item.Tag = i;
                    item.Content = i.ToString();
                    FirstPos.Items.Add(item);
                    if ((int)item.Tag == tag)
                        FirstPos.SelectedItem = item;
                }
            }
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

            if (api.IsConnected)
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
            api.ReplaySearch(TMTVO.Api.ReplaySearchModeTypes.ToEnd, 0);
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
            LiveStandingsModule lsm = api.FindModule("LiveStandings") as LiveStandingsModule;

            int pos = lsm.FindDriver(CameraModule.FollowedDriver).PositionLive + delta;
            string nextPlate = "";

            if (pos < 1)
                nextPlate = lsm.FindLastDriver().Driver.Car.CarNumber;
            else if (pos > lsm.Items.Count)
                nextPlate = lsm.Leader.Driver.Car.CarNumber;
            else
                nextPlate = lsm.FindDriverByPos(pos).Driver.Car.CarNumber;

            if (autoCommit)
                api.SwitchCamera(padCarNum(nextPlate), Convert.ToInt32(cameraSelectComboBox.SelectedValue));
        }

        private void uiCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (uiCheckBox.IsChecked == false)
            {
                int currentCamState = (int)api.GetData("CamCameraState");
                if ((currentCamState & 0x0008) == 0)
                    api.Sdk.BroadcastMessage(BroadcastMessageTypes.CamSetState, (currentCamState | 0x0008), 0);
            }
            else
            {

                //SharedData.showSimUi = false;
            }
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

        private void RevMeter_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.RevMeter.Active)
                f1Window.RevMeterFadeOut();
            else
            {
                int carIdx = CameraModule.FollowedDriver;
                if (carIdx == -1)
                    return;

                LiveStandingsItem driver = ((LiveStandingsModule)api.FindModule("LiveStandings")).FindDriver(carIdx);
                if (driver == null)
                    return;

                f1Window.RevMeterFadeIn();
            }
        }

        private void SpeedCompare_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.SpeedCompareWidget.Active)
                f1Window.SpeedCompFadeOut();
            else
            {
                LiveStandingsModule m = (LiveStandingsModule)api.FindModule("LiveStandings");

                LiveStandingsItem driver1 = null;
                LiveStandingsItem driver2 = null;

                int carIdx = CameraModule.FollowedDriver;
                if (carIdx == -1)
                    return;

                if (SpeedCompMode.SelectedIndex == 0)
                {
                    driver1 = m.FindDriver(carIdx);
                    driver2 = m.FindDriverByPos(driver1.PositionLive + 1);
                }
                else
                {
                    driver2 = m.FindDriver(carIdx);
                    driver1 = m.FindDriverByPos(driver2.PositionLive - 1);
                }

                if (driver1 == null || driver2 == null)
                    return;

                f1Window.SpeedCompFadeIn(driver1, driver2);
            }
        }

        private void DriverInfo_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.DriverInfo.Active)
                f1Window.DriverInfoFadeOut();
            else
                f1Window.DriverInfoFadeIn((DriverInfo.DriverInfoMode)DriverInfoMode.SelectedIndex);
        }

        private void DriverInfoMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (f1Window == null)
                return;

            if (f1Window.DriverInfo.Active)
                f1Window.DriverInfo.SwitchMode((DriverInfo.DriverInfoMode)DriverInfoMode.SelectedIndex);
        }

        private void CanResize_Checked(object sender, RoutedEventArgs e)
        {
            // TODO Implement
        }

        private void CanResize_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO Implement
        }

        private void DriverOverview_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.SideBar.Active && f1Window.SideBar.Mode == SideBarWidget.SideBarMode.DriverOverView)
                f1Window.SideBarFadeOut();
            else if (!f1Window.SideBar.Active)
            {
                LiveStandingsModule lsm = ((LiveStandingsModule)api.FindModule("LiveStandings"));

                int carIdx1 = int.Parse(DriverOverviewDriver1.SelectedValue.ToString());
                int carIdx2 = int.Parse(DriverOverviewDriver2.SelectedValue.ToString());
                f1Window.SideBarFadeInDriverOverview(lsm.FindDriver(carIdx1), lsm.FindDriver(carIdx2));
            }
        }

        private void DriverOverviewDriver1_DropDownOpened(object sender, EventArgs e)
        {
            canUpdateDO1 = false;
        }

        private void DriverOverviewDriver1_DropDownClosed(object sender, EventArgs e)
        {
            canUpdateDO1 = true;
        }

        private void DriverOverviewDriver2_DropDownOpened(object sender, EventArgs e)
        {
            canUpdateDO2 = false;
        }

        private void DriverOverviewDriver2_DropDownClosed(object sender, EventArgs e)
        {
            canUpdateDO2 = true;
        }

        private void FirstPos_DropDownOpened(object sender, EventArgs e)
        {
            canUpdateFp = false;
        }

        private void FirstPos_DropDownClosed(object sender, EventArgs e)
        {
            canUpdateFp = true;
        }

        private void Battle_For_Pos_Click(object sender, RoutedEventArgs e)
        {
            if (f1Window.SideBar.Active && f1Window.SideBar.Mode == SideBarWidget.SideBarMode.BattleForPosition)
                f1Window.SideBarFadeOut();
            else if (!f1Window.SideBar.Active)
                f1Window.SideBarFadeInBattleForPos(int.Parse(FirstPos.SelectedValue.ToString()), int.Parse(NumberOfPositions.SelectedValue.ToString()));
        }

        private void SoF_Click(object sender, RoutedEventArgs e)
        {
            f1Window.JoinFadeIn("Strength of Field", DriverModule.SoF + " SoF");
        }

        private void JoinConv_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HashTag.Text))
                return;

            f1Window.JoinFadeIn("Join the Conversation", HashTag.Text.StartsWith("#") ? HashTag.Text : "#" + HashTag.Text);
        }
    }
}