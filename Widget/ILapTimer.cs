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
        protected static readonly float roadPreviewTime = 0.005F;
        protected static readonly float ovalPreviewTime = 0.002F;

        LiveStandingsItem LapDriver { get; }
        void FadeIn(LiveStandingsItem driver);
        void SectorComplete();
        void LapComplete();
    }
}
