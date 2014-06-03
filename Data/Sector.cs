using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Sector
    {
        public int Number { get; set; }
        public float Time { get; set; }
        public float Speed { get; set; }
        public double Begin { get; set; }

        public Sector()
        {
            this.Number = 0;
            this.Time = 0;
            this.Speed = 0;
            this.Begin = 0;
        }
    }
}
