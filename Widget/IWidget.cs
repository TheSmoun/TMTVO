using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMTVO
{
    public interface IWidget
    {
        bool Active { get; }
        void FadeOut();
        void Tick();
    }
}
