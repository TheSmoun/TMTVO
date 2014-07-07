using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class SessionsModule : Module
    {
        public SessionsModule() : base("Sessions")
        {

        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            throw new NotImplementedException();
        }
    }
}
