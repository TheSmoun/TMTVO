using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsModule : Module
    {
        private RaceBar raceBar;
        public List<LiveStandingsItem> Items { get; private set; }

        public LiveStandingsModule() : base("LiveStandings")
        {
            Items = new List<LiveStandingsItem>();
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
            // TODO
        }
    }
}
