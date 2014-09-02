using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für iRacingControls.xaml
    /// </summary>
    public partial class iRacingControls : Window
    {
        private API api;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private F1TVOverlay mainWindow;
        private Controller.TMTVO tmtvo;
        private DispatcherTimer updateTimer;
        private bool autoCommit;
        private bool cameraUpdate;
        private bool driverUpdate;

        public CameraModule CameraModule { get; set; }
        public DriverModule DriverModule { get; set; }

        public iRacingControls()
        {
            InitializeComponent();
        }

        public iRacingControls(API api, F1TVOverlay mainWindow, Controller.TMTVO t) : this()
        {
            this.api = api;
            this.mainWindow = mainWindow;
            this.tmtvo = t;
            this.autoCommit = true;
            this.driverUpdate = true;
            this.cameraUpdate = true;
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
