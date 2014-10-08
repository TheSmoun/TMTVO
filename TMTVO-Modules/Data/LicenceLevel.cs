using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TMTVO.Data
{
    public enum LicenceLevel
    {
        R,
        D,
        C,
        B,
        A,
        P,
        WC,
        None
    }

    public sealed class LicenceToBrush
    {
        public static Dictionary<LicenceLevel, Brush> RacingLicenceToBrush = new Dictionary<LicenceLevel, Brush>()
        {
            {LicenceLevel.R, new SolidColorBrush(Colors.Red)},
            {LicenceLevel.D, new SolidColorBrush(Colors.Orange)},
            {LicenceLevel.C, new SolidColorBrush(Colors.Yellow)},
            {LicenceLevel.B, new SolidColorBrush(Colors.Green)},
            {LicenceLevel.A, new SolidColorBrush(Colors.Blue)},
            {LicenceLevel.P, new SolidColorBrush(Colors.DarkGray)},
            {LicenceLevel.WC, new SolidColorBrush(Colors.LightGray)}
        };

        public static Dictionary<LicenceLevel, Color> RacingLicenceToColor = new Dictionary<LicenceLevel, Color>()
        {
            {LicenceLevel.R, Colors.Red},
            {LicenceLevel.D, Colors.Orange},
            {LicenceLevel.C, Colors.Yellow},
            {LicenceLevel.B, Colors.Green},
            {LicenceLevel.A, Colors.Blue},
            {LicenceLevel.P, Colors.DarkGray},
            {LicenceLevel.WC, Colors.LightGray}
        };
    }
}
