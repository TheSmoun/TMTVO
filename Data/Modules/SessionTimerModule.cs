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
            sessionTimer.Module = this;
            this.SessionTime = 0;
        }

        public override void Update(ConfigurationSection rootNode)
        {
            if (!sessionTimer.Active)
                return;

            List<Dictionary<string, object>> sessions = rootNode.GetMapList("SessionInfo.Sessions");

            object sessionTime;
            if (sessions[sessions.Count - 1].TryGetValue("SessionTime", out sessionTime) && sessionTime is string)
            {
                string sTime = (string)sessionTime;
                this.SessionTime = (int)float.Parse(sTime.Replace('.', ',').Substring(0, sTime.Length - 4));
            }

            Application.Current.Dispatcher.Invoke(new Action(() => {
                sessionTimer.Tick();
            }));
        }
    }
}
