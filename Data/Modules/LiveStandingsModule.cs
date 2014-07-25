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

        public LiveStandingsItem GetLeader()
        {
            return Items.Find(i => i.Position == 1);
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            if (!liveTiming.Active && !raceBar.Active)
                return;

            ClearComponents();
            Items.Clear();

            List<Dictionary<string, object>> sessions = rootNode.GetMapList("SessionInfo.Sessions");
            Dictionary<string, object> session = sessions[sessions.Count - 1];
            List<Dictionary<string, object>> resultPositions = session.Get("ResultsPositions") as List<Dictionary<string, object>>;
            foreach (Dictionary<string, object> resultPosition in resultPositions)
            {
                int carIdx = int.Parse(resultPosition.GetDictValue("CarIdx"));
                LiveStandingsItem item = new LiveStandingsItem(((DriverModule)api.FindModule("DriverModule")).FindDriver(carIdx));
                this.AddComponent(item);
                item.Update(resultPosition, api);
                Items.Add(item);
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (liveTiming.Active)
                    liveTiming.Tick();

                if (raceBar.Active)
                    raceBar.Tick();
            }));
        }
    }
}
