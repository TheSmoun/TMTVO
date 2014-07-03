using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public static class Utils
    {
        public static int PadCarNum(string input)
        {
            int num = Int32.Parse(input);
            int zero = input.Length - num.ToString().Length;

            int retVal = num;
            int numPlace = 1;
            if (num > 99)
                numPlace = 3;
            else if (num > 9)
                numPlace = 2;
            if (zero > 0)
            {
                numPlace += zero;
                retVal = num + 1000 * numPlace;
            }

            return retVal;
        }
    }
}
