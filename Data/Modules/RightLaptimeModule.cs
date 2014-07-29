using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class RightLaptimeModule : LaptimeModule
    {
        public RightLaptimeModule() : base("RightLapTimer")
        {
            // TODO LapTimer
        }

        public override void Update(ConfigurationSection root, API api)
        {
            throw new NotImplementedException();
        }
    }
}
