using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Ini;
using TMTVO.Data.Modules;
using TMTVO;
using TMTVO_Api.ThemeApi;

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

        public Controls Controls { get; private set; }
        public API Api { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = TMTVO.Instance;
            t.Api = new API(TICKS_PER_SECOND);
            t.Controls = new Controls(t.Api);

            t.InitalizeModules();
            t.Controls.Show();
            return t;
        }

        public void InitalizeModules()
        {
            Api.AddModule(new SessionsModule());
            Api.AddModule(new SessionTimerModule());
            Api.AddModule(new TeamRadioModule());
            Api.AddModule(new DriverModule());
            Api.AddModule(new LiveStandingsModule());
            Api.AddModule(new CameraModule());
            Api.AddModule(new TimeDeltaModule());
            Api.AddModule(new GridModule());
        }
    }
}
