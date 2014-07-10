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
        public TMTVO.Data.Modules.LiveStandingsItem Item { get; private set; }
        public LiveStandingsModule Module { get; set; }

        private float oldTime = float.MaxValue;
        private int oldPosition = int.MaxValue;

        public LiveTimingItem()
        {
            this.InitializeComponent();
        }

        public void LapTimeImproved()
        {
            updateWidget();

            Storyboard sb = FindResource("TimeImproved") as Storyboard;
            sb.Begin();
        }

        public void PositionImproved()
        {
            updateWidget();

            Storyboard sb = FindResource("PositionImproved") as Storyboard;
            sb.Begin();
        }

        private void updateWidget()
        {
            int position = Item.Position;
            float time = Item.FastestLapTime;

            if (position == 1)
            {
                BackgroundLeader.Visibility = Visibility.Visible;
                BackgroundLeader1.Visibility = Visibility.Visible;
                NumberLeader.Visibility = Visibility.Visible;

                int min = (int)(time / 60);
                float sectime = time % 60;
                StringBuilder sb = new StringBuilder();
                if (min > 0)
                    sb.Append(min).Append(':');

                sb.Append(sectime.ToString("00.000"));

                GapText.Text = sb.ToString();
            }
            else
            {
                BackgroundLeader.Visibility = Visibility.Hidden;
                BackgroundLeader1.Visibility = Visibility.Hidden;
                NumberLeader.Visibility = Visibility.Hidden;

                float diff = time - Module.GetLeader().FastestLapTime;
                int min = (int)(diff / 60);
                float secDiff = diff % 60;
                StringBuilder sb = new StringBuilder();
                if (min > 0)
                    sb.Append(min).Append(':');

                sb.Append(secDiff.ToString("00.000"));

                GapText.Text = sb.ToString();
            }

            ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;
            Position.Text = position.ToString();

            oldTime = time;
            oldPosition = position;
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

        public void Tick()
        {
            if (Item.Position < oldPosition)
                PositionImproved();
            else if (Item.FastestLapTime < oldTime)
                LapTimeImproved();
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
}