﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Api
{
    public static class ApiUtils
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

        public static string GetGearValue(this Dictionary<int, string> dict, int key)
        {
            string value = null;
            if (dict.TryGetValue(key, out value))
                return value;

            return string.Empty;
        }

        public static string ConvertToTimeString(this float seconds)
        {
            if (seconds < 0)
                return "NO TIME";

            int min = (int)(seconds / 60);
            float sectime = seconds % 60;
            StringBuilder sb = new StringBuilder();
            if (min > 0)
                sb.Append(min).Append(':').Append(sectime.ToString("00.000"));
            else
                sb.Append(sectime.ToString("0.000"));

            return sb.ToString().Replace(',', '.');
        }
    }
}
