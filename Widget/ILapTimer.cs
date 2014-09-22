using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMTVO.Data.Modules;

namespace TMTVO.Widget.F1
{
    public interface ILapTimer : IWidget
    {
        LiveStandingsItem LapDriver { get; }
        void FadeIn(LiveStandingsItem driver);
        void SectorComplete(float seconds, int index);
        void LapComplete(float seconds);
    }
}
