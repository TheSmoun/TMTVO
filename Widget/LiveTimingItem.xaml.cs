using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaktionslogik für LiveStandingsItem.xaml
    /// </summary>
    public partial class LiveTimingItem : UserControl
    {
        public LiveStandingsItem Item { get; private set; }
        public LiveStandingsModule Module { get; set; }

        public int OldPosition = -1;
        private float oldTime = -1;
        private LiveTimingItemMode mode;

        public LiveTimingItem()
        {
            this.InitializeComponent();
        }

        private void LapTimeImproved()
        {
            UpdateWidget();

            Storyboard sb = FindResource("TimeImproved") as Storyboard;
            sb.Begin();
        }

        private void PositionImproved()
        {
            UpdateWidget();

            Storyboard sb = FindResource("PositionImproved") as Storyboard;
            sb.Begin();
        }

        private void UpdateWidget()
        {
            int position = Item.Position;
            float time = Item.FastestLapTime;

            if (position == 1)
            {
                switch (mode)
                {
                    case LiveTimingItemMode.LastName:
                        GapText.Visibility = Visibility.Hidden;
                        ThreeLetterCode.Text = Item.Driver.LastUpperName;
                        break;
                    default:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;
                        BackgroundLeader.Visibility = Visibility.Visible;
                        BackgroundLeader1.Visibility = Visibility.Visible;
                        NumberLeader.Visibility = Visibility.Visible;

                        int min = (int)(time / 60);
                        float sectime = time % 60;
                        StringBuilder sb = new StringBuilder();
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(sectime.ToString("00.000"));

                        GapText.Text = sb.ToString().Replace(',','.');

                        if (time != oldTime)
                            UpdateDiff();
                        break;
                }
            }
            else
            {
                BackgroundLeader.Visibility = Visibility.Hidden;
                BackgroundLeader1.Visibility = Visibility.Hidden;
                NumberLeader.Visibility = Visibility.Hidden;

                switch (mode)
                {
                    case LiveTimingItemMode.Gap:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;

                        float diff = time - Module.GetLeader().FastestLapTime;
                        int min = (int)(diff / 60);
                        float secDiff = diff % 60;
                        StringBuilder sb = new StringBuilder();
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(secDiff.ToString("0.000"));

                        GapText.Text = sb.ToString().Replace(',', '.');
                        break;
                    case LiveTimingItemMode.Time:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;

                        min = (int)(time / 60);
                        float sectime = time % 60;
                        sb = new StringBuilder();
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(sectime.ToString("00.000"));

                        GapText.Text = sb.ToString().Replace(',', '.');
                        break;
                    case LiveTimingItemMode.LastName:
                        GapText.Visibility = Visibility.Hidden;
                        ThreeLetterCode.Text = Item.Driver.LastUpperName;
                        break;
                }
            }

            Position.Text = position.ToString();
            oldTime = time;
            OldPosition = position;
        }

        public void JoinedPit()
        {
            InPit.Visibility = Visibility.Visible;
        }

        public void LeftPit()
        {
            InPit.Visibility = Visibility.Hidden;
        }

        public void ShowFlag()
        {
            Flag.Visibility = Visibility.Visible;
        }

        public void RemoveFlag()
        {
            Flag.Visibility = Visibility.Hidden;
        }

        public void Tick(LiveStandingsItem item, LiveTimingItemMode mode)
        {
            this.Item = item;
            this.mode = mode;
            this.Module = TMTVO.Controller.TMTVO.Instance.Api.FindModule("LiveStandings") as LiveStandingsModule;

            if (Item.Position < OldPosition)
                PositionImproved();
            else if (Item.FastestLapTime < oldTime)
                LapTimeImproved();
            else
                UpdateWidget();
        }

        public void UpdateDiff()
        {
            float diff = oldTime - Module.GetLeader().FastestLapTime;
            int min = (int)(diff / 60);
            float secDiff = diff % 60;
            StringBuilder sb = new StringBuilder();
            if (min > 0)
                sb.Append(min).Append(':');

            sb.Append(secDiff.ToString("00.000"));

            GapText.Text = sb.ToString();
        }
    }

    public enum LiveTimingItemMode : int
    {
        Gap = 0,
        Time = 1,
        LastName = 2
    }
}