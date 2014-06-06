using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data.Modules
{
    public abstract class LaptimeModule : Module
    {
        protected Driver Driver { get; private set; }

        public LaptimeModule(Driver driver, string name) : base(name)
        {
            this.Driver = driver;
        }
    }
}
