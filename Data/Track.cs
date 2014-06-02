using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Track
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public float Length { get; set; }
        public string DisplayName { get; set; }
        public string DisplayShortName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public float Altitude { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public int NumberOfTurns { get; set; }
        public float PitSpeedLimit { get; set; }
        public TrackType TrackType { get; set; }
        public Weather Weather { get; set; }
    }
}
