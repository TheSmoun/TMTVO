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
        public SessionFlags SessionFlags { get; private set; }

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

            this.TimeRemaining = (int)(double)api.Sdk.GetData("SessionTimeRemain");
            parseFlag((int)api.Sdk.GetData("SessionFlags"));

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
            // TODO Laps Driven.
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
                        SessionType = Data.SessionType.TimeTrial; // TODO
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

        private void parseFlag(int flag)
        {
            /*
            Dictionary<Int32, sessionFlag> flagMap = new Dictionary<Int32, sessionFlag>()
            {
                // global flags
                0x00000001 = sessionFlag.checkered,
                0x00000002 = sessionFlag.white,
                green = sessionFlag.green,
                yellow = 0x00000008,
             * 
                red = 0x00000010,
                blue = 0x00000020,
                debris = 0x00000040,
                crossed = 0x00000080,
             * 
                yellowWaving = 0x00000100,
                oneLapToGreen = 0x00000200,
                greenHeld = 0x00000400,
                tenToGo = 0x00000800,
             * 
                fiveToGo = 0x00001000,
                randomWaving = 0x00002000,
                caution = 0x00004000,
                cautionWaving = 0x00008000,

                // drivers black flags
                black = 0x00010000,
                disqualify = 0x00020000,
                servicible = 0x00040000, // car is allowed service (not a flag)
                furled = 0x00080000,
             * 
                repair = 0x00100000,

                // start lights
                startHidden = 0x10000000,
                startReady = 0x20000000,
                startSet = 0x40000000,
                startGo = 0x80000000,

            };*/

            Int64 regularFlag = flag & 0x0000000f;
            Int64 specialFlag = (flag & 0x0000f000) >> (4 * 3);
            Int64 startlight = (flag & 0xf0000000) >> (4 * 7);

            if (regularFlag == 0x8 || specialFlag >= 0x4)
                SessionFlags = Data.SessionFlags.Yellow;
            else if (regularFlag == 0x2)
                SessionFlags = Data.SessionFlags.White;
            else if (regularFlag == 0x1)
                SessionFlags = Data.SessionFlags.Checkered;
            else
                SessionFlags = Data.SessionFlags.Green;

            //session.StartLight = (SessionStartLights)startlight;
        }
    }
}
