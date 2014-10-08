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


    public static class HelperVoids
    {
        public static string GetDictValue(this Dictionary<string, object> dict, string key)
        {
            object value = null;
            if (dict.TryGetValue(key, out value) && value is string)
                return (string)value;

            return null;
        }

        public static object Get(this Dictionary<string, object> dict, string key)
        {
            object value = null;
            if (dict.TryGetValue(key, out value))
                return value;

            return null;
        }

        public static bool FlagSet(this SessionFlag value, SessionFlag flag)
        {
            return (value & flag) == flag;
        }
    }
}
