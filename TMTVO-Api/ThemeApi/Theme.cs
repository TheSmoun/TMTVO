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
        public IControlPanel Controls { get; private set; }
        public ThemeType Type { get; private set; }
        public bool Active { get; protected set; }

        public Theme(IThemeWindow window, IControlPanel controls, ThemeType type)
        {
            this.Window = window;
            this.Controls = controls;
            this.Type = type;
            this.Active = false;
        }

        public abstract void FadeAllOut();
        public abstract void ShowOverlay();
        public abstract void Reset();
        public abstract void Close();
        public abstract double Fps();
    }
}
