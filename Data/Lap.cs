using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Lap
    {
        public int LapNumber { get; set; }
        public float Time { get; set; }
        public double Begin { get; set; }
        public float Speed { get; set; }
        public int Position { get; set; }
        public int ClassPosition { get; set; }
        public float Gap { get; set; }
        public int GapLaps { get; set; }
        public List<Sector> Sectors { get; set; }
        public int ReplayPos { get; set; }
        public double SessionTime { get; set; }

        public Lap()
        {
            Sectors = new List<Sector>();
        }

        public float GetTimeUntilSector(int index)
        {
            if (index >= Sectors.Count || index < 0)
                return -1;

            float time = 0;
            for (int i = 0; i < index; i++)
                time += Sectors[i].Time;

            return time;
        }
    }
}
