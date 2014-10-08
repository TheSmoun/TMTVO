using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Weather
    {
        public Skies Skies { get; set; }
        public float TrackTemp { get; set; }
        public float AirTemp { get; set; }
        public float WindSpeed { get; set; }
        public int Humidity { get; set; }

        //public float AirPressure { get; set; }
        //public float WindDirection { get; set; }
        //public float FogLevel { get; set; }
    }
}
