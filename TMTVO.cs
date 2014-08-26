﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Ini;
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

        public F1TVOverlay Window { get; private set; }
        public TvoControls TvoControls { get; private set; }
        public iRacingControls iRControls { get; private set; }
        public API Api { get; private set; }
        public IniFile Cars { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = TMTVO.Instance;
            t.Api = new API(TICKS_PER_SECOND);
            t.Window = new F1TVOverlay();
            t.TvoControls = new TvoControls(t.Window, t);
            t.iRControls = new iRacingControls(t.Api, t.Window, t);

            t.InitalizeModules();
            t.TvoControls.Show();
            t.Cars = new IniFile(@"C:\Users\Simon\Documents\TMTVO\TMTVO\Data\Ini\cars.ini"); // TODO Pfad einstellen
            return t;
        }

        public void InitalizeModules()
        {
            Api.AddModule(new SessionsModule(Window.WeatherWidget));
            Api.AddModule(new SessionTimerModule(Window.SessionTimer, Window.LapsRemaining));
            Api.AddModule(new TeamRadioModule(Window.TeamRadio));
            Api.AddModule(new DriverModule(iRControls, Window.ResultsWidget));
            Api.AddModule(new LiveStandingsModule(Window.LiveTiming, Window.RaceBar, Window.ResultsWidget));
            Api.AddModule(new LeftLaptimeModule(Window.LapTimerLeft));
            //Api.AddModule(new CameraModule(iRControls)); //TODO Fix Loop in CameraModule Tick void
            Api.AddModule(new TimeDeltaModule());
        }
    }
}
