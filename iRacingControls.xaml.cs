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

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für iRacingControls.xaml
    /// </summary>
    public partial class iRacingControls : Window
    {
        private API api;
        DateTime cameraUpdate = DateTime.Now;
        DispatcherTimer updateTimer = new DispatcherTimer();
        Boolean autoCommitEnabled = false;

        Thread replayThread;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private MainWindow mainWindow;
        private Controller.TMTVO tmtvo;

        public iRacingControls()
        {
            InitializeComponent();
        }

        public iRacingControls(API api, MainWindow mainWindow, Controller.TMTVO t) : this()
        {
            this.api = api;
            this.mainWindow = mainWindow;
            this.tmtvo = t;
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
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            updateTimer.Tick += new EventHandler(updateControls);
            updateTimer.Start();
            cameraUpdate = DateTime.MinValue;
            cameraSelectComboBox.Items.Clear();
            driverSelect.Items.Clear();
            updateControls(new object(), new EventArgs());
        }

        private void updateControls(object sender, EventArgs e)
        {
            
        }

        private void addBookmark_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cameraSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void commitButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void autoCommitButton_Click(object sender, RoutedEventArgs e)
        {

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

        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void prevDriver_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nextDriver_Click(object sender, RoutedEventArgs e)
        {

        }

        private void uiCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
