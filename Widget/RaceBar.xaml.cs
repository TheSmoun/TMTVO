﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using TMTVO.Data.Modules;
using System.Windows.Media.Animation;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für RaceBar.xaml
	/// </summary>
	public partial class RaceBar : UserControl, IWidget
	{
        private static readonly double PAGE_DURATION_MS = 10000;
        private static readonly double PAGE_SWITCH_COOLDOWN = 500;

        public bool Active { get; private set; }
        public bool Live { get; set; }
        public LiveStandingsModule Module { get; set; }
        public RaceBarMode Mode { get; set; }

        private RaceBarMode oldMode;
        private Timer pageTimer;
        private Timer pageCooldown;
        private int pageIndex;
        private int oldPageIndex;

		public RaceBar()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            Live = false;
            pageIndex = 0;
            oldPageIndex = 0;

            pageTimer = new Timer(PAGE_DURATION_MS);
            pageTimer.Elapsed += SwitchPage;

            pageCooldown = new Timer(PAGE_SWITCH_COOLDOWN);
            pageCooldown.Elapsed += FadeNewPageIn;

            Mode = RaceBarMode.Gap;
        }

        public void FadeIn()
        {
            if (Active)
                return;

            Active = true;
            pageIndex = 0;
            oldPageIndex = 0;
            LoadPage();

            Storyboard sb = FindResource("FadeAllIn") as Storyboard;
            sb.Begin();

            pageTimer.Start();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;

            Storyboard sb = FindResource("FadeAllOut") as Storyboard;
            sb.Begin();

            pageTimer.Stop();
            pageCooldown.Stop();
        }

        public void Tick()
        {
            LoadPage(pageIndex);
        }

        private void SwitchPage(object sender, ElapsedEventArgs e)
        {
            /*if (Module.Items.Count % 5 == 0)
                pageIndex = (pageIndex + 1) / (Module.Items.Count % 5);
            else
                pageIndex = (pageIndex + 1) / ((Module.Items.Count % 5) + 1);
             (1 + 1) * 5 < 10*/

            int i = ((pageIndex + 1) * 5 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (oldPageIndex == i)
                return;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (UIElement elem in RaceBarBackground.Children)
                    ((RaceBarItem)elem).FadeOut();

                pageCooldown.Start();
            }));
        }

        private void FadeNewPageIn(object sender, ElapsedEventArgs e)
        {
            pageCooldown.Stop();
            pageIndex = ((pageIndex + 1) * 5 < Module.Items.Count) ? pageIndex + 1 : 0;

            if (oldPageIndex != pageIndex)
            {
                LoadPage();
                oldPageIndex = pageIndex;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (UIElement elem in RaceBarBackground.Children)
                        ((RaceBarItem)elem).FadeIn();
                }));
            }
        }

        private void LoadPage()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoadPage(pageIndex);
                foreach (UIElement elem in RaceBarBackground.Children)
                    ((RaceBarItem)elem).FadeIn();
            }));
        }

        private void LoadPage(int index)
        {
            if (index == 0)
                oldMode = Mode;

            UIElementCollection items = RaceBarBackground.Children;
            int j = 0;
            for (int i = index; i < index + 5; i++)
            {
                RaceBarItem item = (RaceBarItem)items[j++];
                int pos = j + (index * 5);
                LiveStandingsItem stItem = Module.Items.Find(it => it.Position == pos);
                if (stItem == null)
                {
                    item.Show = false;
                    continue;
                }

                item.Show = true;

                if (stItem.Position == 1)
                    item.NumberLeader.Visibility = Visibility.Visible;
                else
                    item.NumberLeader.Visibility = Visibility.Hidden;

                item.Position.Text = stItem.Position.ToString();

                switch (oldMode)
                {
                    case RaceBarMode.Gap:
                        item.ThreeLetterCode.Visibility = Visibility.Visible;
                        item.GapText.Visibility = Visibility.Visible;
                        item.DriverName.Visibility = Visibility.Hidden;

                        checkPits(item, stItem);

                        item.ThreeLetterCode.Text = stItem.Driver.ThreeLetterCode;
                        if (stItem.Position == 1)
                            item.GapText.Text = "Leader";
                        else
                        {
                            if (stItem.GapLaps == 0)
                                if (Live)
                                    item.GapText.Text = "+" + stItem.GapLiveLeader.ToString("0.000").Replace(',', '.');
                                else
                                    item.GapText.Text = "+" + stItem.GapTime.ToString("0.000").Replace(',', '.');
                            else
                                item.GapText.Text = "+" + stItem.GapLaps.ToString() + (stItem.GapLaps == 1 ? " Lap" : " Laps");
                        }
                        break;
                    case RaceBarMode.Interval:
                        item.ThreeLetterCode.Visibility = Visibility.Visible;
                        item.GapText.Visibility = Visibility.Visible;
                        item.DriverName.Visibility = Visibility.Hidden;

                        checkPits(item, stItem);

                        item.ThreeLetterCode.Text = stItem.Driver.ThreeLetterCode;
                        if (stItem.Position == 1)
                            item.GapText.Text = "Interval";
                        else
                        {
                            float gap = 0;
                            if (Live)
                                gap = stItem.GapLive;
                            else
                            {
                                LiveStandingsItem stItem2 = Module.Items.Find(it => it.Position == (j + (index * 5)) - 1);
                                gap = stItem.GapTime - stItem2.GapTime;
                            }

                            item.GapText.Text = "+" + gap.ToString("0.000").Replace(',', '.');
                        }
                        break;
                    case RaceBarMode.Name:
                        item.ThreeLetterCode.Visibility = Visibility.Hidden;
                        item.GapText.Visibility = Visibility.Hidden;
                        item.PitText.Visibility = Visibility.Hidden;
                        item.DriverName.Visibility = Visibility.Visible;

                        item.DriverName.Text = stItem.Driver.LastUpperName;
                        break;
                    default:
                        break;
                }
            }
        }

        private void checkPits(RaceBarItem raceBarItem, LiveStandingsItem liveStandingsItem)
        {
            if (liveStandingsItem.InPits)
            {
                raceBarItem.PitText.Visibility = Visibility.Visible;
                raceBarItem.GapText.Visibility = Visibility.Hidden;
            }
            else
            {
                raceBarItem.PitText.Visibility = Visibility.Hidden;
                raceBarItem.GapText.Visibility = Visibility.Visible;
            }
        }

        public enum RaceBarMode : int
        {
            Gap = 0,
            Interval = 1,
            Name = 2
        }
    }
}