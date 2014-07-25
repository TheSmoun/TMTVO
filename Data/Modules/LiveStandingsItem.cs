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
        public Driver Driver { get; private set; }
        public List<Lap> Laps { get; private set; }
        public Lap CurrentLap { get; private set; }
        public int Position { get; private set; }
        public int OldPosition { get; private set; }
        public int ClassPosition { get; private set; }
        public float FastestLapTime { get; private set; }
        public int FastestLapNumber { get; private set; }
        public float LastLapTime { get; private set; }
        public float GapTime { get; private set; }
        public int GapLaps { get; private set; }
        public int LapsLed { get; private set; }
        public int LapsComplete { get; private set; }
        public int Incidents { get; private set; }
        public double LapBegin { get; private set; }
        public bool InPits { get; private set; }

        public LiveStandingsItem(Driver driver)
        {
            this.Driver = driver;
            Laps = new List<Lap>();
        }

        public void Update(Dictionary<string, object> dict, API api)
        {
            OldPosition = Position;

            int carIdx = int.Parse(dict.GetDictValue("CarIdx"));
            if (Driver.CarIndex != carIdx)
                return;

            Position = int.Parse(dict.GetDictValue("Position"));
            ClassPosition = int.Parse(dict.GetDictValue("ClassPosition")) + 1;
            GapLaps = int.Parse(dict.GetDictValue("Lap"));
            GapTime = float.Parse(dict.GetDictValue("Time").Replace('.', ','));
            FastestLapNumber = int.Parse(dict.GetDictValue("FastestLap"));
            FastestLapTime = float.Parse(dict.GetDictValue("FastestTime").Replace('.', ','));
            LastLapTime = float.Parse(dict.GetDictValue("LastTime").Replace('.', ','));
            LapsLed = int.Parse(dict.GetDictValue("LapsLed"));
            LapsComplete = int.Parse(dict.GetDictValue("LapsComplete"));
            Incidents = int.Parse(dict.GetDictValue("Incidents"));

            SurfaceType type = ((SurfaceType[])api.GetData("CarIdxTrackSurface"))[carIdx];
            InPits = (((bool[])api.GetData("CarIdxOnPitRoad"))[carIdx] || type == SurfaceType.InPitStall || type == SurfaceType.NotInWorld);
        }
    }
}
