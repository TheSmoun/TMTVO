using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO_Api.ThemeApi
{
    public abstract class Theme
    {
        public IThemeWindow Window { get; private set; }

        public Theme(IThemeWindow window)
        {
            this.Window = window;
        }

        public abstract void FadeAllOut();
    }
}
