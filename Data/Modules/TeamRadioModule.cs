using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TeamRadioModule : Module
    {
        public int SpeekingCarIndex { get; private set; }
        private bool canShowTeamRadio;

        public TeamRadioModule() : base("TeamRadio")
        {
            this.SpeekingCarIndex = -1;
            this.CanShowTeamRadio = false;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            SpeekingCarIndex = (int)api.GetData("RadioTransmitCarIdx");
        }

        public bool CanShowTeamRadio
        {
            get { return canShowTeamRadio; }
            set
            {
                /*if (value == canShowTeamRadio)
                    return;

                if (!value && teamRadio.Active)
                    teamRadio.FadeOut();
                else if (value && SpeekingCarIndex != -1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        teamRadio.Tick();
                    }));
                */
                canShowTeamRadio = value;
            }
        }

        public override void Reset()
        {
            this.SpeekingCarIndex = -1;
            this.CanShowTeamRadio = false;
        }
    }
}
