﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
using TMTVO.Data.Modules;

namespace TMTVO.Widget.F1
{
	/// <summary>
	/// Interaktionslogik für LapTimer.xaml
	/// </summary>
	public partial class LapTimerLeft : UserControl, ILapTimer
	{
        protected static readonly float roadPreviewTime = 0.005F;
        protected static readonly float ovalPreviewTime = 0.002F;

        private Timer updateCd;
        private bool canUpdate;

        private bool gapVisible;
        private bool posVisible;

        public LiveStandingsItem LapDriver { get; private set; }
        public bool Active { get; private set; }

		public LapTimerLeft()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            canUpdate = false;
            gapVisible = false;
            posVisible = false;
        }

        public void FadeIn(LiveStandingsItem driver)
        {
            if (Active)
                return;

            this.LapDriver = driver;
            this.Active = true;
            this.canUpdate = true;

            this.DriversName.Text = driver.Driver.LastUpperName;
            this.DriversNumber.Text = driver.Driver.NumberPlateInt.ToString();
            this.NumberPlate.Fill = new SolidColorBrush(driver.Driver.LicColor);

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
            sb.Begin();

            if (gapVisible)
            {
                gapVisible = false;
                Storyboard sb1 = FindResource("HideGap") as Storyboard;
                sb1.Begin();
            }

            if (posVisible)
            {
                posVisible = false;
                Storyboard sb2 = FindResource("HideNumber") as Storyboard;
                sb2.Begin();
            }

            LapDriver = null;
        }

        public void SectorComplete()
        {
            if (!Active)
                return;

            canUpdate = false;
            updateCd = new System.Timers.Timer(5400);
            updateCd.Elapsed += TimerElapsed;
            updateCd.Start();

            float gap = LapDriver.FastestLapTime - LapDriver.LastLapTime;
            if (gap >= 0)
            {
                BackgroundGreen.Visibility = Visibility.Hidden;
                GapTime.Text = '+' + gap.ToString("0.000");
            }
            else
            {
                BackgroundGreen.Visibility = Visibility.Visible;
                GapTime.Text = gap.ToString("0.000");
            }


            float seconds = LapDriver.CurrentLap.Time;

            float s = seconds % 60;
            int m = (int)(seconds / 60);

            StringBuilder sbu = new StringBuilder();
            if (m != 0)
                sbu.Append(m).Append(":");

            if (s < 10 && m != 0)
                sbu.Append("0");

            sbu.Append(s.ToString("0.000"));
            TimeText.Text = sbu.ToString();

            gapVisible = true;
            Storyboard sb = FindResource("ShowGap") as Storyboard;
            sb.Begin();
        }

        public void LapComplete()
        {
            if (!Active || LapDriver.CurrentLap.Time < 0.100)
                return;

            SectorComplete();

            int position = LapDriver.Position;
            if (position > 1)
            {
                BackgroundLeader.Visibility = Visibility.Hidden;
                BackgroundNumber.Visibility = Visibility.Hidden;
                One.Visibility = Visibility.Hidden;
            }
            else
            {
                BackgroundLeader.Visibility = Visibility.Visible;
                BackgroundNumber.Visibility = Visibility.Visible;
                One.Visibility = Visibility.Visible;
            }

            Position.Text = position.ToString();

            posVisible = true;
            Storyboard sb = FindResource("ShowNumber") as Storyboard;
            sb.Begin();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            updateCd.Stop();
            canUpdate = true;

            if (gapVisible)
            {
                gapVisible = false;
                Storyboard sb = FindResource("HideGap") as Storyboard;
                sb.Begin();
            }

            if (posVisible)
            {
                posVisible = false;
                Storyboard sb = FindResource("HideNumber") as Storyboard;
                sb.Begin();
            }
        }

        public void Tick()
        {
            if (!canUpdate)
                return;

            float seconds = LapDriver.CurrentLap.Time;
            if (seconds < 0)
                return;

            float s = seconds % 60;
            int m = (int)(seconds / 60);

            StringBuilder sb = new StringBuilder();
            if (m != 0)
                sb.Append(m).Append(":");

            if (s < 10 && m != 0)
                sb.Append("0");

            sb.Append(s.ToString("0.0").Replace(',', '.'));
            sb.Append("    ");

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (canUpdate)
                    TimeText.Text = sb.ToString();
            }));

            List<float> Sectors = ((SessionsModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("Sessions")).Track.Sectors;
            for (int i = 0; i < Sectors.Count; i++)
            {
                float sector = Sectors[i];
                if (sector < 0.1F)
                    continue;

                if (LapDriver.PrevTrackPct <= sector - LapTimerLeft.roadPreviewTime && LapDriver.CurrentTrackPct > sector - LapTimerLeft.roadPreviewTime)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        BackgroundRed.Visibility = Visibility.Visible;
                    
                    // TOTO Set sector text

                        Storyboard sb1 = FindResource("ShowGap") as Storyboard;
                        sb1.Begin();
                    }));
                }
            }

            // TODO Call sector- and lap-complete-methods.
        }
    }
}