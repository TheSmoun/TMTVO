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

        public override void Update(Node rootNode)
        {
            throw new NotImplementedException();
        }
    }
}
