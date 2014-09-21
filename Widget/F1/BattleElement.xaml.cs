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
using TMTVO.Widget;

namespace TMTVO
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

        public BattleElement(SideBarWidget widget, LiveStandingsModule module)
        {
            this.InitializeComponent();

            this.widget = widget;
            this.module = module;
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

            Position.Text = Driver.PositionLive.ToString("0");
            if (Driver.PositionLive > widget.FirstPos)
            {
                float gap = 0;
                LiveStandingsItem nextDriver = module.FindDriverByPos(Driver.PositionLive - 1);
                if (nextDriver != null)
                    gap = Driver.GapLive - nextDriver.GapLive;

                GapText.Text = gap.ConvertToTimeString();
            }
        }

        public void Reset()
        {
            Active = false;
            Driver = null;
        }
    }
}