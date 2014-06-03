using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.Data;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für LapTimer.xaml
	/// </summary>
	public partial class LapTimerLeft : UserControl, ILapTimer
	{
        private System.Timers.Timer updateCd;
        private bool canUpdate;

        public Stopwatch Stopwatch { get { return LapDriver.Stopwatch; } }
        public Thread Thread { get; private set; }
        public ResultItem LapDriver { get; private set; }
        public bool Active { get; private set; }

		public LapTimerLeft()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            canUpdate = false;
        }

        public void FadeIn(ResultItem driver)
        {
            if (Active)
                return;

            this.LapDriver = driver;
            this.Active = true;
            this.canUpdate = true;

            this.DriversName.Text = driver.Driver.LastUpperName;
            this.DriversNumber.Text = driver.Driver.Car.CarNumber;
            // TODO ClassColor

            this.Thread = new Thread(Run);
            TimeText.Text = "0.0    ";
            Thread.Start();

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            this.Active = false;
            this.canUpdate = true;

            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin(); // TODO Pos und Gap Ausblenden
        }

        public void SectorComplete()
        {
            if (!Active)
                return;

            canUpdate = false;
            updateCd = new System.Timers.Timer(5400);
            updateCd.Elapsed += TimerElapsed;
            updateCd.Start();

            float gap = -0.234f; // TODO get gap.
            if (gap >= 0)
            {
                BackgroundGreen.Visibility = Visibility.Hidden;
                Gap.Text = '+' + gap.ToString("0.000");
            }
            else
            {
                BackgroundGreen.Visibility = Visibility.Visible;
                Gap.Text = gap.ToString("0.000");
            }

            float seconds = ((float)LapDriver.Stopwatch.ElapsedMilliseconds) / 1000f;

            float s = seconds % 60;
            int m = (int)(seconds / 60);

            StringBuilder sbu = new StringBuilder();
            if (m != 0)
                sbu.Append(m).Append(":");

            if (s < 10 && m != 0)
                sbu.Append("0");

            sbu.Append(s.ToString("0.000"));
            TimeText.Text = sbu.ToString();

            Storyboard sb = FindResource("ShowGap") as Storyboard;
            sb.Begin();
        }

        public void LapComplete()
        {
            if (!Active || LapDriver.Stopwatch.ElapsedMilliseconds < 100)
                return;

            SectorComplete();

            int position = LapDriver.Position;
            if (position > 1)
                BackgroundLeader.Visibility = Visibility.Hidden;
            else
                BackgroundLeader.Visibility = Visibility.Visible;

            Position.Text = position.ToString();

            Storyboard sb = FindResource("ShowNumber") as Storyboard;
            sb.Begin();

            LapDriver.Stopwatch.Restart();
        }

        private void Run()
        {
            while (Active)
            {
                if (!canUpdate)
                    continue;

                float seconds = ((float)LapDriver.Stopwatch.ElapsedMilliseconds) / 1000f;
                if (seconds < 0)
                    return;

                float s = seconds % 60;
                int m = (int)(seconds / 60);

                StringBuilder sb = new StringBuilder();
                if (m != 0)
                    sb.Append(m).Append(":");

                if (s < 10 && m != 0)
                    sb.Append("0");

                sb.Append(s.ToString("0.0"));
                sb.Append("    ");

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (canUpdate)
                        TimeText.Text = sb.ToString();
                }));
            }

            LapDriver = null;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            updateCd.Stop();
            canUpdate = true;
        }
    }
}