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
        public SideBarMode Mode;

        private List<ISideBarElement> elements;

		public SideBarWidget()
		{
			this.InitializeComponent();
            elements = new List<ISideBarElement>();
		}

        public void FadeInDriverOverview(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (driver1 == null && driver2 == null)
                return;

            this.Mode = SideBarMode.DriverOverView;
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

            title.VerticalAlignment = VerticalAlignment.Top;
            stops.VerticalAlignment = VerticalAlignment.Top;
            best.VerticalAlignment = VerticalAlignment.Top;
            last.VerticalAlignment = VerticalAlignment.Top;

            elements.Add(title);
            elements.Add(stops);
            elements.Add(best);
            elements.Add(last);

            title.Margin = new Thickness(0, 0 + i * 180, 0, 0);
            stops.Margin = new Thickness(0, 36 + i * 180, 0, 0);
            best.Margin = new Thickness(0, 72 + i * 180, 0, 0);
            last.Margin = new Thickness(0, 108 + i * 180, 0, 0);

            LayoutRoot.Children.Add(title);
            LayoutRoot.Children.Add(stops);
            LayoutRoot.Children.Add(best);
            LayoutRoot.Children.Add(last);

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
            LayoutRoot.Children.Clear();
        }

        public enum SideBarMode
        {
            DriverOverView,
            BattleForPosition,
            Improvements
        }
    }
}