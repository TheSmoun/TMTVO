using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsItem : Component
    {
        public Driver Driver { get; set; }
        public int Position { get; private set; }
        public int ClassPosition { get; private set; }
        public float FastestLapTime { get; private set; }
        public int FastestLapNumber { get; private set; }
        public float LastLapTime { get; private set; }
        public float GapTime { get; private set; }
        public int GapLaps { get; private set; }
        public int LapsLed { get; private set; }
        public int LapsComplete { get; private set; }
        public int Incidents { get; private set; }

        public LiveStandingsItem(Driver driver)
        {
            this.Driver = driver;
        }

        public void Update(Dictionary<string, object> dict)
        {
            int carIdx = int.Parse(dict.GetDictValue("CarIdx"));
            if (Driver.CarIndex != carIdx)
                return;

            Position = int.Parse(dict.GetDictValue("Position"));
            ClassPosition = int.Parse(dict.GetDictValue("ClassPosition")) + 1;
            GapLaps = int.Parse(dict.GetDictValue("Lap"));
            GapTime = float.Parse(dict.GetDictValue("Time"));
            FastestLapNumber = int.Parse(dict.GetDictValue("FastestLap"));
            FastestLapTime = float.Parse(dict.GetDictValue("FastestTime"));
            LastLapTime = float.Parse(dict.GetDictValue("LastTime"));
            LapsLed = int.Parse(dict.GetDictValue("LapsLed"));
            LapsComplete = int.Parse(dict.GetDictValue("LapsComplete"));
            Incidents = int.Parse(dict.GetDictValue("Incidents"));
        }
    }
}
