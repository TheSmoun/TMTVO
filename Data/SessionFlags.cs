using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    [Flags]
    public enum SessionFlag : long
    {
        // global flags
        Checkered = 0x1,
        White = 0x2,
        Green = 0x4,
        Yellow = 0x8,
        Red = 0x10,
        Blue = 0x20,
        Debris = 0x40,
        Crossed = 0x80,
        YellowWaving = 0x100,
        OneLapToGreen = 0x200,
        GreenHeld = 0x400,
        TenToGo = 0x800,
        FiveToGo = 0x1000,
        RandomWaving = 0x2000,
        Caution = 0x4000,
        CautionWaving = 0x8000,

        // drivers black flags
        Black = 0x10000,
        Disqualify = 0x20000,
        Servicible = 0x40000, // car is allowed service (not a flag)
        Furled = 0x80000,
        Repair = 0x100000,

        // start lights
        StartHidden = 0x10000000,
        StartReady = 0x20000000,
        StartSet = 0x40000000,
        startGo = 0x80000000,

        // invalid
        Invalid = 0
    };
}
