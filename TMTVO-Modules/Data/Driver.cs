using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TMTVO.Api;

namespace TMTVO.Data
{
    public class Driver
    {
        public Driver()
        { 
            this.Car = new Car();
        }

        public int CarIndex { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Initials { get; set; }
        public int UserId { get; set; }
        public int IRating { get; set; }
        public int SafetyRating { get; set; }
        public LicenceLevel Licence { get; set; }
        public string ClubName { get; set; }
        public string Division { get; set; }
        public Car Car { get; set; }
        public bool Spectator { get; set; }
        public Color LicColor { get; set; }
        public int NumberPlateInt
        {
            get
            {
                if (Car.CarNumber != null)
                    return int.Parse(Car.CarNumber);
                else return 0;
            }
        }

        public int NumberPlatePadded
        {
            get
            {
                if (Car.CarNumber != null) 
                    return Utils.PadCarNum(Car.CarNumber);
                else 
                    return -1;
            }
        }

        public string SrString
        {
            get
            {
                if (SafetyRating == -1)
                    return "Unknown";

                return Licence.ToString() + ((double)SafetyRating / 100).ToString("0.00");
            }
        }

        public override string ToString()
        {
            return "#" + Car.CarNumber + " " + LastUpperName;
        }

        public string LastUpperName
        {
            get
            {
                string lastName = FullName;
                string[] names = lastName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names[names.Length - 1].StartsWith("Jr", true, CultureInfo.CurrentCulture) || names[names.Length - 1].StartsWith("Sr", true, CultureInfo.CurrentCulture))
                    lastName = names[names.Length - 2] + " " + names[names.Length - 1];
                else
                    lastName = names[names.Length - 1];

                return lastName.ToUpper();
            }
        }

        public string ThreeLetterCode
        {
            get
            {
                return this.Initials.ToUpper() + LastUpperName[1];
            }
        }

        public void Update(Dictionary<string, object> info, API api)
        {
            foreach (KeyValuePair<string, object> kv in info)
            {
                switch (kv.Key)
                {
                    case "CarIdx":
                        CarIndex = int.Parse((string)kv.Value);
                        break;
                    case "UserName":
                        FullName = (string)kv.Value;
                        break;
                    case "Initials":
                        Initials = (string)kv.Value;
                        break;
                    case "UserID":
                        UserId = int.Parse((string)kv.Value);
                        break;
                    case "CarNumber":
                        Car.CarNumber = ((string)kv.Value).Substring(1, ((string)kv.Value).Length - 2);
                        break;
                    case "CarPath":
                        Car.CarName = (string)kv.Value;
                        break;
                    case "CarClassID":
                        Car.CarClassId = int.Parse((string)kv.Value);
                        break;
                    case "CarID":
                        Car.CarId = int.Parse((string)kv.Value);
                        break;
                    case "CarClassShortName":
                        Car.CarClassShortName = (string)kv.Value;
                        break;
                    case "CarClassRelSpeed":
                        Car.CarClassRelSpeed = int.Parse((string)kv.Value);
                        break;
                    case "CarClassLicenseLevel":
                        Car.CarClassLicenceLevel = int.Parse((string)kv.Value);
                        break;
                    case "IRating":
                        IRating = int.Parse((string)kv.Value);
                        break;
                    case "LicColor":
                        LicColor = (Color)ColorConverter.ConvertFromString("#FF" + kv.Value.ToString().Substring(2));
                        break;
                    case "LicLevel":
                        int licLevel = int.Parse((string)kv.Value);
                        switch (licLevel)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                Licence = LicenceLevel.R;
                                break;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                Licence = LicenceLevel.D;
                                break;
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                Licence = LicenceLevel.C;
                                break;
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                                Licence = LicenceLevel.B;
                                break;
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                                Licence = LicenceLevel.A;
                                break;
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                                Licence = LicenceLevel.P;
                                break;
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                                Licence = LicenceLevel.WC;
                                break;
                            default:
                                Licence = LicenceLevel.None;
                                break;
                        }
                        break;
                    case "LicSubLevel":
                        SafetyRating = int.Parse((string)kv.Value);
                        break;
                    case "ClubName":
                        ClubName = (string)kv.Value;
                        break;
                    case "DivisionName":
                        Division = (string)kv.Value;
                        break;
                }
            }
        }
    }
}
