using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class SessionTimerModule : Module
    {
        private SessionTimer sessionTimer;

        public int SessionTime { get; private set; }
        public int LapsTotal { get; private set; }
        public int LapsDriven { get; private set; }
        public SessionType SessionType { get; private set; }

        public SessionTimerModule(SessionTimer sessionTimer) : base("SessionTimer")
        {
            this.sessionTimer = sessionTimer;
            this.SessionTime = 0;
        }

        public override void Update(ConfigurationSection rootNode)
        {
            if (!sessionTimer.Active)
                return;

            // TODO implement
        }
    }
}
