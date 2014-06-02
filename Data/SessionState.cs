using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMTVO.Data
{
    public enum SessionState
    {
        Invalid,
        Gridding,
        Warmup,
        Pacing,
        Racing,
        Checkered,
        Cooldown
    }
}
