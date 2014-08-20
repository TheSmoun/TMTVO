﻿using System;
using System.Collections.Generic;
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
using TMTVO.Data.Modules;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für ResultsWidget.xaml
	/// </summary>
	public partial class ResultsWidget : UserControl, IWidget
	{

        public static readonly double PAGE_SWITCH_COOLDOWN = 500;
        public static readonly int MS_PER_PAGE = 10000;

        public bool Active { get; private set; }
        public LiveStandingsModule Module { get; set; }
        public ResultsMode Mode { get; private set; }

        private Timer pageTimer;
        private Timer pageCooldown;
        private int pageIndex;

		public ResultsWidget()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            pageIndex = 0;

            pageTimer = new Timer();
            pageTimer.Elapsed += SwitchPage;

            pageCooldown = new Timer(PAGE_SWITCH_COOLDOWN);
            pageCooldown.Elapsed += FadeNewPageIn;
        }

        private void SwitchPage(object sender, ElapsedEventArgs e)
        {
            int i = ((pageIndex + 1) * 12 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (pageIndex >= i)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FadeOut();
                }));
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = FindResource("FadeOutPos") as Storyboard;
                sb.Begin();
                pageCooldown.Start();
            }));
        }

        private void FadeNewPageIn(object sender, ElapsedEventArgs e)
        {
            pageCooldown.Stop();
            pageIndex = ((pageIndex + 1) * 5 < Module.Items.Count) ? pageIndex + 1 : 0;
            LoadPage();
        }

        public void Show(int msPerPage, ResultsMode mode)
        {
            Mode = mode;
            pageIndex = 0;
            pageTimer.Interval = (double)msPerPage;
            
            Storyboard sb = FindResource("FadeInHeader") as Storyboard;
            sb.Begin();

            LoadPage();
            pageTimer.Start();
        }

        private void LoadPage()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoadPage(pageIndex);
                FadeInPositions();
            }));
        }

        private void LoadPage(int pageIndex)
        {
            UIElementCollection items = Positions.Children;
            int j = 0;
            for (int i = pageIndex; i < pageIndex + 12; i++)
            {
                ResultsItem item = (ResultsItem)items[j++];
                int pos = j + (pageIndex * 12);
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
                item.DriversNumber.Text = stItem.Driver.NumberPlateInt.ToString();
                item.DriverName.Text = stItem.Driver.LastUpperName;
                item.TeamCarName.Text = stItem.Driver.Car.CarName; // TODO Car

                switch (Mode)
                {
                    case ResultsMode.Practice:
                        item.FadeInColorP.Visibility = Visibility.Hidden;
                        item.PointsItem.Visibility = Visibility.Hidden;

                        if (pos == 1)
                            item.Time.Text = stItem.FastestLapTimeSting;
                        else
                        {
                            float diff = stItem.FastestLapTime - Module.Leader.FastestLapTime;
                            if (diff < 0)
                                item.Time.Text = "No Time";
                            else
                            {
                                int min = (int)(diff / 60);
                                float secDiff = diff % 60;
                                StringBuilder sb = new StringBuilder("+");
                                if (min > 0)
                                    sb.Append(min).Append(':');

                                sb.Append(secDiff.ToString("0.000"));
                                item.Time.Text = sb.ToString().Replace(',', '.');
                            }
                        }

                        break;
                    case ResultsMode.Race:
                        item.FadeInColorP.Visibility = Visibility.Visible;
                        item.PointsItem.Visibility = Visibility.Visible;

                        if (pos == 1)
                            item.Time.Text = "Winner";
                        else
                        {
                            if (stItem.GapLaps == 0)
                                item.Time.Text = "+" + stItem.GapTime.ToString("0.000").Replace(',', '.');
                            else
                                item.Time.Text = "+" + stItem.GapLaps.ToString() + (stItem.GapLaps == 1 ? " Lap" : " Laps");
                        }

                        // TODO calculate Points

                        break;
                    default:
                        break;
                }
            }
        }

        private void FadeInPositions()
        {
            UIElementCollection uiE = Positions.Children;
            for (int i = 0; i < uiE.Count; i++)
                ((ResultsItem)uiE[i]).FadeOut();
            
            Storyboard sb = FindResource("SetMargin") as Storyboard;
            sb.Begin();

            for (int i = 0; i < uiE.Count; i++)
                ((ResultsItem)uiE[i]).FadeInLater(i * 25);
        }

        public void FadeOut()
        {
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public enum ResultsMode
        {
            Practice,
            Race
        }
    }
}