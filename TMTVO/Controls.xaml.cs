using iRSDKSharp;
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
using TMTVO_Api.ThemeApi;
using TMTVO_F1Theme;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für Controls.xaml
    /// </summary>
    public partial class Controls : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private API api;
        private Theme theme;
        private DispatcherTimer timer;
        private DispatcherTimer updateTimer;

        public Controls(API api)
        {
            this.api = api;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += timer_Tick;
            timer.Start();
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            api.Run = false;
            theme.Close();
            Application.Current.Shutdown(0);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!api.IsConnected && theme != null && theme.Active)
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

            if (api != null)
                MsItem.Content = api.LastMS + " MS";

            if (theme != null)
                FpsItem.Content = theme.Fps().ToString("0.0") + " FPS";
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (theme == null)
            {
                switch (ThemeSelector.SelectedIndex)
                {
                    case 0:
                        theme = F1Theme.Load();
                        ControlsBorder.Child = theme.Controls as F1ControlTabs;
                        break;
                    default:
                        return;
                }

                ThemeSelector.IsEnabled = false;
                StartStop.Content = "Stop";
                api.Run = true;
                theme.ShowOverlay();
            }
            else
            {
                ControlsBorder.Child = null;
                theme.Close();
                theme = null;
                StartStop.Content = "Start";
                api.Run = false;
                ThemeSelector.IsEnabled = true;
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            updateTimer.Tick += api.Connect;
            updateTimer.Tick += api.UpdateControls;
            updateTimer.Start();
        }
    }
}
