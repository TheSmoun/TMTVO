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

        public int TimeTotal { get; private set; }
        public int LapsTotal { get; private set; }
        public int LapsDriven { get; private set; }
        public int TimeRemaining { get; private set; }
        public SessionType SessionType { get; private set; }

        public SessionTimerModule(SessionTimer sessionTimer) : base("SessionTimer")
        {
            this.sessionTimer = sessionTimer;
            sessionTimer.Module = this;
            this.TimeTotal = 0;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            if (!sessionTimer.Active)
                return;

            TimeRemaining = (int)(double)api.Sdk.GetData("SessionTimeRemain");

            List<Dictionary<string, object>> sessions = rootNode.GetMapList("SessionInfo.Sessions");
            Dictionary<string, object> session = sessions[sessions.Count - 1];
            object sessionLaps;
            if (session.TryGetValue("SessionLaps", out sessionLaps) && sessionLaps is string)
            {
                string laps = (string)sessionLaps;
                if (laps.StartsWith("unlimited"))
                    LapsTotal = -1;
                else
                    LapsTotal = int.Parse(laps);
            }

            object sessionTime;
            if (session.TryGetValue("SessionTime", out sessionTime) && sessionTime is string)
            {
                string time = ((string)sessionTime).Substring(0, ((string)sessionTime).Length - 4).Replace('.', ',');
                TimeTotal = (int)float.Parse(time);
            }

            object sessionType;
            if (session.TryGetValue("SessionType", out sessionType) && sessionType is string)
            {
                string type = (string)sessionType;
                switch (type)
                {
                    case "Offline Testing":
                        SessionType = Data.SessionType.OfflineTesting;
                        break;
                    case "Practice":
                        SessionType = Data.SessionType.Practice;
                        break;
                    case "Qualifying":
                        SessionType = Data.SessionType.Qualifying;
                        break;
                    case "Race":
                        if (LapsTotal == -1)
                            SessionType = Data.SessionType.TimeRace;
                        else
                            SessionType = Data.SessionType.LapRace;
                        break;
                    case "Time Trial":
                        SessionType = Data.SessionType.Practice; // TODO
                        break;
                    default:
                        SessionType = Data.SessionType.None;
                        break;
                }
            }


            Application.Current.Dispatcher.Invoke(new Action(() => {
                sessionTimer.Tick();
            }));
        }
    }
}
