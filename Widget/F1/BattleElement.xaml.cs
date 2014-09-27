using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für BattleElement.xaml
	/// </summary>
	public partial class BattleElement : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }
        public LiveStandingsItem Driver { get; internal set; }

        private SideBarWidget widget;
        private LiveStandingsModule module;
        private BattleElementMode mode;

        private System.Timers.Timer cooldownTimer;

        public BattleElement(SideBarWidget widget, LiveStandingsModule module)
        {
            this.InitializeComponent();

            this.widget = widget;
            this.module = module;
            this.mode = BattleElementMode.Default;
            cooldownTimer = new System.Timers.Timer(5000);
            cooldownTimer.AutoReset = true;
            cooldownTimer.Elapsed += cooldownTimer_Elapsed;
        }

        private void cooldownTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImpTriangle.Visibility = LostTriangle.Visibility = Visibility.Hidden;
                GapText.Visibility = Visibility.Visible;
                mode = BattleElementMode.Default;
            }));
        }

        internal void FadeIn(LiveStandingsItem item, int delay)
        {
            if (Active || item == null)
                return;

            Driver = item;
            Active = true;

            Thread t = new Thread(fadeInLater);
            t.Start(delay);
        }

        private void fadeInLater(object obj)
        {
            Thread.Sleep((int)obj);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                (FindResource("FadeIn") as Storyboard).Begin();
            }));
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            (FindResource("FadeOut") as Storyboard).Begin();
        }

        public void Tick()
        {
            if (Driver == null)
            {
                FadeOut();
                return;
            }

            if (Driver.PositionLive == 1)
                NumberLeader.Visibility = Visibility.Visible;
            else
                NumberLeader.Visibility = Visibility.Hidden;

            Position.Text = Driver.PositionLive.ToString("0");
            ThreeLetterCode.Text = Driver.Driver.ThreeLetterCode;
            ClassColorNormal.Color = ClassColorLeader.Color = Driver.Driver.LicColor;

            if (Driver.PositionLive > widget.FirstPos)
            {
                float gap = 0;
                gap =  Driver.GapLive;

                GapText.Text = gap.ConvertToTimeString();
            }
            else
                GapText.Text = string.Empty;

            if (Driver.PositionImprovedBattleFor && mode != BattleElementMode.PositionImproved)
            {
                mode = BattleElementMode.PositionImproved;
                ImpTriangle.Visibility = Visibility.Visible;
                GapText.Visibility = Visibility.Hidden;
                Driver.PositionImprovedBattleFor = false;
                cooldownTimer.Start();
            }
            else if (Driver.PositionLostBattleFor && mode != BattleElementMode.PositionLost)
            {
                mode = BattleElementMode.PositionLost;
                LostTriangle.Visibility = Visibility.Visible;
                GapText.Visibility = Visibility.Hidden;
                Driver.PositionLostBattleFor = false;
                cooldownTimer.Start();
            }
        }

        public void Reset()
        {
            Active = false;
            Driver = null;
        }

        public enum BattleElementMode
        {
            PositionImproved,
            PositionLost,
            Default
        }
    }
}