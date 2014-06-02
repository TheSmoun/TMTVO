using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public enum TriggerType
    {
        FlagGreen,
        FlagYellow,
        FlagWhite,
        FlagCheckered,
        LightsOff,
        LightsReady,
        LightsSet,
        LightsGo,
        Replay,
        Live,
        RadioOn,
        RadioOff,
        FastestLap,
        OffTrack,
        NotInWorld,
        PitIn,
        PitOut,
        PitOccupied,
        PitEmpty,
        Init
    }
}
