using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Modules;

namespace TMTVO.Controller
{
    public class TMTVO
    {
        private static readonly int TICKS_PER_SECOND = 20;

        public MainWindow Window { get; private set; }
        public TvoControls TvoControls { get; private set; }
        public iRacingControls iRControls { get; private set; }
        public IAPI Api { get; private set; }

        public SessionTimerModule sessionTimerModule { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = new TMTVO();
            //t.Api = new API(TICKS_PER_SECOND);
            t.Api = new SimpleApi(TICKS_PER_SECOND);
            t.Window = new MainWindow();
            t.InitalizeModules();

            t.TvoControls = new TvoControls(t.Window, t);
            t.TvoControls.Show();

            t.iRControls = new iRacingControls(t.Api, t.Window, t);
            t.iRControls.Show();

            return t;
        }

        public void InitalizeModules()
        {
            Api.AddModule((this.sessionTimerModule = new SessionTimerModule(Window.SessionTimer)));
        }
    }
}
