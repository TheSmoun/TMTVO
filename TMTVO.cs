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
        private static TMTVO instance = null;

        public static TMTVO Instance
        {
            get
            {
                if (instance == null)
                    instance = new TMTVO();

                return instance;
            }
        }

        private static readonly int TICKS_PER_SECOND = 20;

        public MainWindow Window { get; private set; }
        public TvoControls TvoControls { get; private set; }
        public iRacingControls iRControls { get; private set; }
        public API Api { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = TMTVO.Instance;
            t.Api = new API(TICKS_PER_SECOND);
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
            Api.AddModule(new SessionsModule());
            Api.AddModule(new SessionTimerModule(Window.SessionTimer));
            Api.AddModule(new TeamRadioModule(Window.TeamRadio));
            Api.AddModule(new DriverModule());
            Api.AddModule(new LiveStandingsModule(Window.LiveTiming, Window.RaceBar));
            Api.AddModule(new LeftLaptimeModule(Window.LapTimerLeft));
        }
    }
}
