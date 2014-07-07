using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;

namespace TMTVO.Data.Modules
{
    public class SessionsModule : Module
    {
        public SessionsModule() : base("Sessions")
        {

        }

        public override void Update(Yaml.ConfigurationSection rootNode)
        {
            Yaml.ConfigurationSection section = null;
            throw new NotImplementedException();
        }
    }
}
