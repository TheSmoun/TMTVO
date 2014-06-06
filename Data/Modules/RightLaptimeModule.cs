using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class RightLaptimeModule : LaptimeModule
    {
        public RightLaptimeModule(Driver driver) : base(driver, "RightLapTimer")
        {
            // TODO LapTimer
        }

        public override void Update(Node rootNode)
        {
            throw new NotImplementedException();
        }
    }
}
