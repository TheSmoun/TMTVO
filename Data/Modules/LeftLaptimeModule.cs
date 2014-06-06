using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Widget;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class LeftLaptimeModule : LaptimeModule
    {
        private LapTimerLeft lapTimer;

        public LeftLaptimeModule(LapTimerLeft lapTimer, Driver driver) : base(driver, "LeftLapTimer")
        {
            this.lapTimer = lapTimer;
        }

        public override void Update(Node rootNode)
        {
            throw new NotImplementedException();
        }
    }
}
