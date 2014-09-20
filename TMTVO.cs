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
        public Controls Controls { get; private set; }
        public API Api { get; private set; }
        public IniFile Cars { get; private set; }

        private TMTVO() { }

        public static TMTVO Launch()
        {
            TMTVO t = TMTVO.Instance;
            t.Api = new API(TICKS_PER_SECOND);
            t.Controls = new Controls(t.Api, t);
            t.Window = new F1TVOverlay();
            t.Controls.f1Window = t.Window;

            t.InitalizeModules();
            t.Controls.Show();
            t.Cars = new IniFile(Environment.CurrentDirectory + @"\cars.ini"); // TODO Pfad einstellen
            return t;
        }

        public void InitalizeModules()
        {
            Api.AddModule(new SessionsModule(Window.WeatherWidget));
            Api.AddModule(new SessionTimerModule(Window.SessionTimer, Window.LapsRemaining));
            Api.AddModule(new TeamRadioModule(Window.TeamRadio));
            Api.AddModule(new DriverModule(Controls, Window.ResultsWidget));
            Api.AddModule(new LiveStandingsModule(Window.LiveTimingWidget, Window.RaceBar, Window.ResultsWidget, Window.DriverInfo, Window.SideBar));
            Api.AddModule(new LeftLaptimeModule(Window.LapTimerLeft));
            Api.AddModule(new CameraModule(Controls));
            Api.AddModule(new TimeDeltaModule());
            Api.AddModule(new RevMeterModule(Window.RevMeter, Window.SpeedCompareWidget));
            Api.AddModule(new GridModule());
        }
    }
}
