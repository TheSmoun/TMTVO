using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TeamRadioModule : Module
    {
        private TeamRadio teamRadio;
        public int SpeekingCarIndex { get; private set; }
        private bool canShowTeamRadio;

        public TeamRadioModule(TeamRadio teamRadio) : base("TeamRadio")
        {
            this.teamRadio = teamRadio;
            this.SpeekingCarIndex = -1;
            this.CanShowTeamRadio = false;
            teamRadio.Module = this;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            SpeekingCarIndex = (int)api.GetData("RadioTransmitCarIdx");
            //if (canShowTeamRadio)
                //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    teamRadio.Tick();
                //}));
        }

        public bool CanShowTeamRadio
        {
            get { return canShowTeamRadio; }
            set
            {
                if (value == canShowTeamRadio)
                    return;

                if (!value && teamRadio.Active)
                    teamRadio.FadeOut();
                else if (value && SpeekingCarIndex != -1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        teamRadio.Tick();
                    }));

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
