using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Trigger
    {
        public int CarIdx { get; private set; }
        public TriggerType Type { get; private set; }

        public Trigger(int carIdx, TriggerType type)
        {
            this.CarIdx = carIdx;
            this.Type = type;
        }
    }
}
