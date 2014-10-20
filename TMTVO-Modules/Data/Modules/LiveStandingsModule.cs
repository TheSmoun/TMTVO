using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsModule : Module
    {
        public List<LiveStandingsItem> Items { get; private set; }

        public LiveStandingsModule() : base("LiveStandings")
        {
            Items = new List<LiveStandingsItem>();
        }

        public LiveStandingsItem FindDriver(int CarIndex)
        {
            return Items.Find(i => i.Driver.CarIndex == CarIndex);
        }

        public LiveStandingsItem FindDriverByPos(int position)
        {
            return Items.Find(i => i.PositionLive == position);
        }

        public LiveStandingsItem FindDriverByPosNL(int position)
        {
            return Items.Find(i => i.PositionLive == position);
        }

        public LiveStandingsItem FindLastDriver()
        {
            LiveStandingsItem item = null;
            foreach (LiveStandingsItem i in Items)
            {
                if (item == null || i.PositionLive > item.PositionLive)
                    item = i;
            }

            return item;
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

        public List<LiveStandingsItem> OrderByTopSpeed()
        {
            return Items.OrderByDescending(i => i.TopSpeed).ToList();
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
        }

        public void UpdateLivePositions()
        {
            int i = 1;
            IEnumerable<LiveStandingsItem> query;

            SessionTimerModule stm = API.Instance.FindModule("SessionTimer") as SessionTimerModule;
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
