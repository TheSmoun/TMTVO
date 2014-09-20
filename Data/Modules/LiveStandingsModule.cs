using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsModule : Module
    {
        private RaceBar raceBar;
        private LiveTimingWidget liveTiming;
        private ResultsWidget results;
        private DriverInfo driverInfo;
        private SideBarWidget sideBar;

        public List<LiveStandingsItem> Items { get; private set; }

        public LiveStandingsModule(LiveTimingWidget liveTiming, RaceBar raceBar, ResultsWidget results, DriverInfo driverInfo, SideBarWidget sideBar) : base("LiveStandings")
        {
            Items = new List<LiveStandingsItem>();

            this.liveTiming = liveTiming;
            this.liveTiming.Module = this;

            this.raceBar = raceBar;
            this.raceBar.Module = this;

            this.results = results;
            this.results.Module = this;

            this.driverInfo = driverInfo;
            this.sideBar = sideBar;
        }

        public LiveStandingsItem FindDriver(int CarIndex)
        {
            return Items.Find(i => i.Driver.CarIndex == CarIndex);
        }

        public LiveStandingsItem FindDriverByPos(int position)
        {
            return Items.Find(i => i.PositionLive == position);
        }

        public bool ContainsItem(int index)
        {
            return Items.Find(i => i.Driver.CarIndex == index) != null;
        }

        public LiveStandingsItem Leader
        {
            get
            {
                return Items.Find(i => i.PositionLive == 1);
            }
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            List<Dictionary<string, object>> sessions = rootNode.GetMapList("SessionInfo.Sessions");
            Dictionary<string, object> session = sessions[sessions.Count - 1];
            List<Dictionary<string, object>> resultPositions = session.Get("ResultsPositions") as List<Dictionary<string, object>>;
            foreach (Dictionary<string, object> resultPosition in resultPositions)
            {
                string s = resultPosition.GetDictValue("CarIdx");
                if (s == null)
                    continue;

                int carIdx = int.Parse(s);
                LiveStandingsItem item = Items.Find(i => i.Driver.CarIndex == carIdx);
                if (item == null)
                {
                    item = new LiveStandingsItem(((DriverModule)api.FindModule("DriverModule")).FindDriver(carIdx));
                    this.AddComponent(item);
                    Items.Add(item);
                }

                item.Update(resultPosition, api, this);
            }

            UpdateLivePositions();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (liveTiming.Active)
                    liveTiming.Tick();

                if (raceBar.Active)
                    raceBar.Tick();

                if (driverInfo.Active)
                    driverInfo.Tick();

                if (sideBar.Active)
                    sideBar.Tick();
            }));
        }

        public void UpdateLivePositions()
        {
            int i = 1;
            IEnumerable<LiveStandingsItem> query;

            SessionTimerModule stm = TMTVO.Controller.TMTVO.Instance.Api.FindModule("SessionTimer") as SessionTimerModule;
            if ((stm.SessionType == SessionType.LapRace || stm.SessionType == SessionType.TimeRace) && stm.SessionState == SessionState.Racing)
            {
                query = Items.OrderByDescending(s => s.CurrentTrackPct);
                foreach (LiveStandingsItem si in query)
                    si.PositionLive = i++;
            }
            else
            {
                query = Items.OrderBy(s => s.Position);
                foreach (LiveStandingsItem si in query)
                    si.PositionLive = si.Position;
            }
        }

        public override void Reset()
        {
            Items.Clear();
        }
    }
}
