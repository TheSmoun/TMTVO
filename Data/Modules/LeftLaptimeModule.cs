using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LeftLaptimeModule : LaptimeModule
    {
        private LapTimerLeft lapTimer;

        public LeftLaptimeModule(LapTimerLeft lapTimer) : base( "LeftLapTimer")
        {
            this.lapTimer = lapTimer;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            if (!lapTimer.Active)
                return;

            lapTimer.Tick();
        }

        public override void Reset()
        {
            // TODO Implement
        }
    }
}
