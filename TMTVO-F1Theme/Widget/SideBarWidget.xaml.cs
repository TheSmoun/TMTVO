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
using TMTVO.Api;
using TMTVO.Data.Modules;
using TMTVO_Api.ThemeApi;
using TMTVO_F1Theme;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für SideBarWidget.xaml
	/// </summary>
	public partial class SideBarWidget : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public IThemeWindow ParentWindow { get; private set; }
        public int FirstPos { get; private set; }
        public int Count { get; private set; }
        public SideBarMode Mode { get; private set; }

        private List<ISideBarElement> elements;
        private LiveStandingsModule module;
        private DriverModule drivers;

		public SideBarWidget(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;
            elements = new List<ISideBarElement>();
		}

        public void FadeInDriverOverview(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (driver1 == null && driver2 == null)
                return;

            module = (LiveStandingsModule)API.Instance.FindModule("LiveStandings");
            this.Mode = SideBarMode.DriverOverView;
            Active = true;
            if (driver1 != null)
                addDriverOverviewElement(driver1, 0);

            if (driver2 != null)
                addDriverOverviewElement(driver2, 1);
        }

        private void addDriverOverviewElement(LiveStandingsItem driver, int i)
        {
            DriverOverviewTitle title = new DriverOverviewTitle(ParentWindow);
            DriverOverviewStops stops = new DriverOverviewStops(ParentWindow);
            LapTimeElement best = new LapTimeElement(ParentWindow);
            LapTimeElement last = new LapTimeElement(ParentWindow);

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

        public void FadeInBattleForPos(int pos, int count)
        {
            this.FirstPos = pos;
            this.Count = count;

            module = (LiveStandingsModule)API.Instance.FindModule("LiveStandings");
            foreach (LiveStandingsItem item in module.Items)
                item.PositionImprovedBattleFor = item.PositionLostBattleFor = false;

            Mode = SideBarMode.BattleForPosition;
            Active = true;
            SideBarTitle title = new SideBarTitle(ParentWindow);
            string t = "BATTLE FOR ";
            if (pos == 1)
                t += "1st";
            else if (pos == 2)
                t += " 2nd";
            else if (pos == 3)
                t += " 3rd";
            else
                t += pos.ToString("0") + "th";

            title.VerticalAlignment = VerticalAlignment.Top;
            LayoutRoot.Children.Add(title);
            elements.Add(title);
            title.FadeIn(t);

            int j = 1;
            for (int i = pos; i < pos + count; i++)
            {
                LiveStandingsItem item = module.FindDriverByPos(i);
                if (item == null)
                    break;

                BattleElement e = new BattleElement(ParentWindow, this, module);
                e.VerticalAlignment = VerticalAlignment.Top;
                elements.Add(e);
                LayoutRoot.Children.Add(e);
                e.Margin = new Thickness(0, j * 36, 0, 0);
                e.FadeIn(item, j * 25);

                j++;
            }
        }

        public void FadeInTopSpeeds()
        {
            if (Active)
                return;

            module = (LiveStandingsModule)API.Instance.FindModule("LiveStandings");
            Mode = SideBarMode.TopSpeed;
            SideBarTitle title = new SideBarTitle(ParentWindow);
            title.VerticalAlignment = VerticalAlignment.Top;
            LayoutRoot.Children.Add(title);
            elements.Add(title);
            title.FadeIn("TOP SPEEDS kph");

            List<LiveStandingsItem> query = module.OrderByTopSpeed();
            for (int i = 0; i < 10; i++)
            {
                LiveStandingsItem item = query[i];
                if (item.TopSpeed > 0)
                {
                    SpeedElement se = new SpeedElement(ParentWindow, SpeedElement.SpeedElementMode.TopSpeed);
                    se.VerticalAlignment = VerticalAlignment.Top;
                    se.Margin = new Thickness(0, (i + 1) * 36, 0, 0);
                    LayoutRoot.Children.Add(se);
                    elements.Add(se);
                    se.FadeIn(i + 1, item, (i + 1) * 25);
                }
            }

            Active = true;
        }

        public void FadeIniRatingGains()
        {
            if (Active)
                return;

            module = (LiveStandingsModule)API.Instance.FindModule("LiveStandings");
            drivers = (DriverModule)API.Instance.FindModule("DriverModule");
            Mode = SideBarMode.iRating;
            SideBarTitle title = new SideBarTitle(ParentWindow);
            title.VerticalAlignment = VerticalAlignment.Top;
            LayoutRoot.Children.Add(title);
            elements.Add(title);
            title.FadeIn("LIVE iRATING GAINS");

            for (int i = 0; i < Math.Min(drivers.DriversCount, 22); i++)
            {
                LiveStandingsItem item = module.FindDriverByPos(i + 1);
                if (item != null)
                {
                    SpeedElement se = new SpeedElement(ParentWindow, SpeedElement.SpeedElementMode.iRating);
                    se.VerticalAlignment = VerticalAlignment.Top;
                    se.Margin = new Thickness(0, (i + 1) * 36, 0, 0);
                    LayoutRoot.Children.Add(se);
                    elements.Add(se);
                    se.FadeIn(i + 1, item, (i + 1) * 25);
                }
            }

            Active = true;
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            foreach (ISideBarElement e in elements)
                e.FadeOut();

            elements.Clear();
        }

        public void Tick()
        {
            if (Mode == SideBarMode.BattleForPosition)
            {
                int j = 1;
                for (int i = FirstPos; i < FirstPos + Count; i++)
                {
                    BattleElement e = elements[j++] as BattleElement;
                    if (e != null && module != null)
                        e.Driver = module.FindDriverByPos(i);
                }
            }

            if (Mode == SideBarMode.TopSpeed)
            {
                int k = 1;
                List<LiveStandingsItem> query = module.OrderByTopSpeed();
                for (int i = 0; i < 10; i++)
                {
                    if (elements.Count <= k)
                        break;

                    SpeedElement se = elements[k++] as SpeedElement;
                    if (se != null)
                    {
                        se.Driver = query[i];
                        se.TopSpeedPosition = i + 1;
                    }
                }
            }

            if (Mode == SideBarMode.iRating)
            {
                int k = 1;
                for (int i = 0; i < 10; i++)
                {
                    if (elements.Count <= k)
                        break;

                    SpeedElement se = elements[k++] as SpeedElement;
                    if (se != null)
                    {
                        se.Driver = module.FindDriverByPos(i + 1);
                        se.TopSpeedPosition = i + 1;
                    }
                }
            }

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
            Improvements,
            TopSpeed,
            iRating
        }
    }
}