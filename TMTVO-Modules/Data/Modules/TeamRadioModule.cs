using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TeamRadioModule : Module
    {
        public int SpeekingCarIndex { get; private set; }
        public bool CanShowTeamRadio { get; set; }

        public TeamRadioModule() : base("TeamRadio")
        {
            this.SpeekingCarIndex = -1;
            this.CanShowTeamRadio = false;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            SpeekingCarIndex = (int)api.GetData("RadioTransmitCarIdx");
        }

        public override void Reset()
        {
            this.SpeekingCarIndex = -1;
            this.CanShowTeamRadio = false;
        }
    }
}
