using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Data;
using TMTVO.Data.Modules;

namespace TMTVO.Widget
{
    public interface ILapTimer : IWidget
    {
        LiveStandingsItem LapDriver { get; }
        void FadeIn(LiveStandingsItem driver);
        void SectorComplete(float seconds, int index);
        void LapComplete(float seconds);
    }
}
