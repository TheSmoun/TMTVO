using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsModule : Module
    {
        private RaceBar raceBar;
        public LiveStandingsItem[] Items { get; set; }

        public LiveStandingsModule() : base("LiveStandings")
        {

        }

        public LiveStandingsItem FindDriver(int CarIndex)
        {
            for (int i = 0; i < Items.Length; i++)
                if (Items[i].Driver.CarIndex == CarIndex)
                    return Items[i];

            return null;
        }

        public override void Update(ConfigurationSection rootNode)
        {
            throw new NotImplementedException();
        }
    }
}
