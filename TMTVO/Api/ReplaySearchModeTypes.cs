using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Api
{
    public enum ReplaySearchModeTypes
    {
        ToStart = 0,
        ToEnd = 1,
        PreviousSession = 2,
        NextSession = 3,
        PreviousLap = 4,
        NextLap = 5,
        PreviousFrame = 6,
        NextFrame = 7,
        PreviousIncident = 8,
        NextIncident = 9,
    }
}
