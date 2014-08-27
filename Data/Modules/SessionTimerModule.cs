using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class SessionTimerModule : Module
    {
        private SessionTimer sessionTimer;
        private LapsRemainingWidget lapsRemaining;

        public int TimeTotal { get; private set; }
        public int LapsTotal { get; private set; }
        public int LapsDriven { get; private set; }
        public int TimeRemaining { get; private set; }
        public SessionType SessionType { get; private set; }
        public SessionFlag SessionFlags { get; private set; }
        public SessionState SessionState { get; private set; }
        public int CautionLaps { get; set; }

        public SessionTimerModule(SessionTimer sessionTimer, LapsRemainingWidget lapsRemaining) : base("SessionTimer")
        {
            this.sessionTimer = sessionTimer;
            sessionTimer.Module = this;
            this.lapsRemaining = lapsRemaining;
            this.TimeTotal = 0;
            CautionLaps = 0;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            SessionState = (SessionState)api.GetData("SessionState");

            List<Dictionary<string, object>> sessions = rootNode.GetMapList("SessionInfo.Sessions");
            Dictionary<string, object> session = sessions[sessions.Count - 1];

            object sessionLaps;
            if (session.TryGetValue("SessionLaps", out sessionLaps) && sessionLaps is string)
            {
                string laps = (string)sessionLaps;
                if (laps.StartsWith("unlimited"))
                    LapsTotal = int.MaxValue;
                else
                    LapsTotal = int.Parse(laps);
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
                        if (LapsTotal == int.MaxValue)
                            SessionType = Data.SessionType.TimeRace;
                        else
                            SessionType = Data.SessionType.LapRace;
                        break;
                    case "Time Trial":
                        SessionType = Data.SessionType.TimeTrial;
                        break;
                    default:
                        SessionType = Data.SessionType.None;
                        break;
                }
            }

            if (!sessionTimer.Active)
                return;

            this.TimeRemaining = (int)(double)api.Sdk.GetData("SessionTimeRemain");
            SessionFlag newFlag = (SessionFlag)Enum.Parse(typeof(SessionFlag), ((int)api.Sdk.GetData("SessionFlags")).ToString(), true);
            if (newFlag.FlagSet(SessionFlag.White) && !SessionFlags.FlagSet(SessionFlag.White))
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    lapsRemaining.FadeIn(1);
                }));

            SessionFlags = newFlag;

            int lapsRemain = (int)api.Sdk.GetData("SessionLapsRemain");
            if (lapsRemain <= 5 && lapsRemain > 1)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    lapsRemaining.FadeIn(lapsRemain);
                }));

            this.LapsDriven = LapsTotal - lapsRemain;
            object sessionTime;
            if (session.TryGetValue("SessionTime", out sessionTime) && sessionTime is string)
            {
                string time = ((string)sessionTime).Substring(0, ((string)sessionTime).Length - 4).Replace('.', ',');
                if (time.StartsWith("unlim"))
                    TimeTotal = int.MaxValue;
                else
                    TimeTotal = (int)float.Parse(time);
            }

            Application.Current.Dispatcher.Invoke(new Action(() => {
                sessionTimer.Tick();
            }));
        }

        public override void Reset()
        {
            TimeTotal = 0;
            LapsTotal = 0;
            LapsDriven = 0;
            TimeRemaining = 0;
            SessionType = SessionType.None;
            SessionFlags = SessionFlag.Invalid;
            SessionState = SessionState.Invalid;
            CautionLaps = 0;
        }
    }
}
