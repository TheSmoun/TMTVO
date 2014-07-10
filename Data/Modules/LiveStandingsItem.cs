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
        public float FastestLapTime { get; private set; }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
