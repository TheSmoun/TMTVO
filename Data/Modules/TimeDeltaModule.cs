using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TimeDeltaModule : Module
    {
        private static readonly float deltaDistance = 10f;
        private static readonly int drivers = 64;

        public TimeDelta TimeDelta { get; private set; }

        private float trackLength;

        public TimeDeltaModule() : base("TimeDelta")
        {
            TimeDelta = null;
            trackLength = -1F;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            Track track = ((SessionsModule)api.FindModule("Sessions")).Track;
            if (track == null)
                return;

            if (TimeDelta == null || track.Length != trackLength)
            {
                TimeDelta = new TimeDelta(track.Length, deltaDistance, drivers);
                trackLength = track.Length;
            }

            TimeDelta.Update(api.CurrentTime, (float[])api.GetData("CarIdxLapDistPct"));
        }

        public override void Reset()
        {
            TimeDelta = null;
        }
    }
}
