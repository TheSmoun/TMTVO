using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TeamRadioModule : Module
    {
        private TeamRadio teamRadio;
        private int oldCarIdx;

        public TeamRadioModule(TeamRadio teamRadio) : base("TeamRadio")
        {
            this.teamRadio = teamRadio;
            this.oldCarIdx = -1;
        }

        public override void Update(ConfigurationSection rootNode)
        {
            if (!teamRadio.Active)
                return;

            // TODO TeamRadio Updaten
            throw new NotImplementedException();
        }
    }
}
