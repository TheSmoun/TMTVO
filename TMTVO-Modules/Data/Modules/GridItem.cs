using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;

namespace TMTVO.Data.Modules
{
    public class GridItem : Component
    {
        public int Position { get; private set; }
        public float QualiTime { get; private set; }
        public int CarIndex { get; private set; }

        public void Update(Dictionary<string, object> dict, API api, Module caller)
        {
            this.CarIndex = int.Parse(dict.GetDictValue("CarIdx"));
            this.Position = int.Parse(dict.GetDictValue("Position")) + 1;
            this.QualiTime = float.Parse(dict.GetDictValue("FastestTime").Replace('.', ','));
        }
    }
}
