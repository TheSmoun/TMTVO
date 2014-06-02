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
        Stopwatch StopWatch { get; }
        Driver Driver { get; set; }
        void Update();
    }
}
