using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Data;
using TMTVO.Data.Modules;

namespace TMTVO.Controller
{
    public class TMTVO
    {
        public MainWindow Window { get; private set; }
        public Controls Controls { get; private set; }
        public API Api { get; private set; }

        public SessionTimerModule sessionTimerModule { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = new TMTVO();
            t.Api = new API(20);
            t.Window = new MainWindow();
            t.InitalizeModules();

            t.Controls = new Controls(t.Window, t);
            t.Controls.Show();

            return t;
        }

        public void InitalizeModules()
        {
            Api.AddModule((this.sessionTimerModule = new SessionTimerModule(Window.SessionTimer)));
        }
    }
}
