using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TMTVO.Data
{
    public enum Skies
    {
        [SkiesStringValue("Clear")]
        Clear,

        [SkiesStringValue("Partly Cloudy")]
        PartlyCloudy,

        [SkiesStringValue("Mostly Cloudy")]
        MostlyCloudy,

        [SkiesStringValue("Overcast")]
        Overcast,

        [SkiesStringValue("!INVALID!")]
        Invalid
    }

    public class SkiesStringValue : Attribute
    {
        public string Value { get; private set; }

        public SkiesStringValue(string value)
        {
            Value = value;
        }
    }

    public class SkiesImageName : Attribute
    {
        public string Value { get; private set; }

        public SkiesImageName(string value)
        {
            Value = value;
        }
    }

    public static class SkiesEnum
    {
        public static string GetStringValue(this Enum value)
        {
            string output = null;
            Type type = value.GetType();
            FieldInfo fi = type.GetField(value.ToString());
            SkiesStringValue[] attrs = fi.GetCustomAttributes(typeof(SkiesStringValue), false) as SkiesStringValue[];
            if (attrs.Length > 0)
                output = attrs[0].Value;

            return output;
        }

        public static BitmapImage GetImageValue(this Enum value)
        {
            Skies skies = (Skies)value;
            BitmapImage image = null;

            switch(skies)
            {
                case Skies.Clear:
                    image = new BitmapImage(new Uri(@"pack://application:,,,/TMTVO;component/Images/skies_clear.png"));
                    break;
                case Skies.PartlyCloudy:
                    image = new BitmapImage(new Uri(@"pack://application:,,,/TMTVO;component/Images/skies_partly_cloudy.png"));
                    break;
                case Skies.MostlyCloudy:
                    image = new BitmapImage(new Uri(@"pack://application:,,,/TMTVO;component/Images/skies_mostly_cloudy.png"));
                    break;
                case Skies.Overcast:
                    image = new BitmapImage(new Uri(@"pack://application:,,,/TMTVO;component/Images/skies_overcast.png"));
                    break;
                default:
                    break;
            }

            return image;
        }
    }
}