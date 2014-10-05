using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        private Timer cooldown;
        private bool canUpdate;

        public TimeDeltaModule() : base("TimeDelta")
        {
            TimeDelta = null;
            trackLength = -1F;
            cooldown = new Timer(10000);
            cooldown.Elapsed += cooldown_Elapsed;
            cooldown.Start();
            canUpdate = true;
        }

        private void cooldown_Elapsed(object sender, ElapsedEventArgs e)
        {
            canUpdate = true;
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

            if (canUpdate)
            {
                TimeDelta.Update(api.CurrentTime, (float[])api.GetData("CarIdxLapDistPct"));
                canUpdate = false;
            }
        }

        public override void Reset()
        {
            TimeDelta = null;
        }
    }
}
