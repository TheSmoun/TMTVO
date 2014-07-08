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
            Dictionary<string, object> map = rootNode.GetMapList("RadioInfo.Radios")[0];
            object radioObject = null;
            if (!map.TryGetValue("Frequencies", out radioObject) || !(radioObject is List<Dictionary<string, object>>))
                return;

            Dictionary<string, object> radio = ((List<Dictionary<string, object>>)radioObject)[0];
            object car = null;
            if (map.TryGetValue("CarIdx", out car) && car is string)
                SpeekingCarIndex = int.Parse((string)car);

            if (canShowTeamRadio)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    teamRadio.Tick();
                }));
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
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        teamRadio.Tick();
                    }));

                canShowTeamRadio = value;
            }
        }
    }
}
