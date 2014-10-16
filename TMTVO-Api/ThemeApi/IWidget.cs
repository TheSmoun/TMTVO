using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMTVO_Api.ThemeApi;

namespace TMTVO
{
    public interface IWidget
    {
        bool Active { get; }
        IThemeWindow ParentWindow { get; }
        void FadeOut();
        void Tick();
        void Reset();
    }
}
