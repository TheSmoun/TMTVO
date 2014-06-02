using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Data;

namespace TMTVO.Widget
{
    public interface ILapTimer : IWidget
    {
        Stopwatch Stopwatch { get; }
        ResultItem LapDriver { get; }

        void FadeIn(ResultItem driver);
        void SectorComplete();
        void LapComplete();
    }
}
