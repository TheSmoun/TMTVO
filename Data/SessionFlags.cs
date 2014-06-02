using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public enum SessionFlags
    {
        // global flags
        Checkered,
        White,
        Green,
        Yellow,
        Red,
        Blue,
        Debris,
        Crossed,
        YellowWaving,
        OneLapToGreen,
        GreenHeld,
        TenToGo,
        FiveToGo,
        RandomWaving,
        Caution,
        CautionWaving,

        // drivers black flags
        Black,
        Disqualify,
        Servicible, // car is allowed service (not a flag)
        Furled,
        Repair,

        // start lights
        StartHidden,
        StartReady,
        StartSet,
        startGo,

        // invalid
        Invalid
    };
}
