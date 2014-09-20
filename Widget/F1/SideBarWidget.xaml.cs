using System;
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
using TMTVO.Data.Modules;
using TMTVO.Widget;

namespace TMTVO
{
	/// <summary>
	/// Interaktionslogik für SideBarWidget.xaml
	/// </summary>
	public partial class SideBarWidget : UserControl, IWidget
	{
        public bool Active { get; private set; }

        private List<ISideBarElement> elements;
        private SideBarMode mode;
        private LiveStandingsItem driverOverview1;
        private LiveStandingsItem driverOverview2;

		public SideBarWidget()
		{
			this.InitializeComponent();
		}

        public void FadeInDriverOverview(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (driver1 == null && driver2 == null)
                return;

            this.mode = SideBarMode.DriverOverView;
            Active = true;
            if (driver1 != null)
                addDriverOverviewElement(driver1, 0);

            if (driver2 != null)
                addDriverOverviewElement(driver2, 1);
        }

        private void addDriverOverviewElement(LiveStandingsItem driver, int i)
        {
            DriverOverviewTitle title = new DriverOverviewTitle();
            DriverOverviewStops stops = new DriverOverviewStops();
            LapTimeElement best = new LapTimeElement();
            LapTimeElement last = new LapTimeElement();

            elements.Add(title);
            elements.Add(stops);
            elements.Add(best);
            elements.Add(last);

            title.FadeIn(driver, 0 + i * 125);
            stops.FadeIn(driver, 25 + i * 125);
            best.FadeIn("BEST", driver, 50 + i * 125);
            last.FadeIn("LAST", driver, 75 + i * 125);
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            foreach (ISideBarElement e in elements)
                e.FadeOut();
        }

        public void Tick()
        {
            foreach (ISideBarElement e in elements)
                e.Tick();
        }

        public void Reset()
        {
            Active = false;
            elements.Clear();
            driverOverview1 = null;
            driverOverview2 = null;
        }

        public enum SideBarMode
        {
            DriverOverView,
            BattleForPosition,
            Improvements
        }
    }
}