using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    [Flags]
    public enum SessionStartLights : int
    {
        Off = 1,    // hidden
        Ready = 2,  // off
        Set = 4,    // red
        Go = 8     // green
    }
}
