using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsModule : Module
    {
        private RaceBar raceBar;
        private LiveTimingWidget liveTiming;
        public List<LiveStandingsItem> Items { get; private set; }

        public LiveStandingsModule(LiveTimingWidget liveTiming, RaceBar raceBar) : base("LiveStandings")
        {
            Items = new List<LiveStandingsItem>();

            this.liveTiming = liveTiming;
            this.liveTiming.Module = this;

            this.raceBar = raceBar;
            this.raceBar.Module = this;
        }

        public LiveStandingsItem FindDriver(int CarIndex)
        {
            return Items.Find(i => i.Driver.CarIndex == CarIndex);
        }

        public LiveStandingsItem FindDriverByPos(int position)
        {
            return Items.Find(i => i.Position == position);
        }

        public LiveStandingsItem GetLeader()
        {
            return Items.Find(i => i.Position == 1);
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

                item.Update(resultPosition, api);
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (liveTiming.Active)
                    liveTiming.Tick();

                if (raceBar.Active)
                    raceBar.Tick();
            }));
        }

        public override void Reset()
        {
            Items.Clear();
        }
    }
}
