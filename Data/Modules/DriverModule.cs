using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class DriverModule : Module
    {
        public List<Driver> Drivers { get; private set; }
        public int CamCarIndex { get; private set; }

        private iRacingControls window;

        public DriverModule(iRacingControls window, ResultsWidget results) : base("DriverModule")
        {
            Drivers = new List<Driver>();
            this.window = window;
            window.DriverModule = this;
            CamCarIndex = -1;

            results.DriverModule = this;
        }

        public Driver FindDriver(int CarIdx)
        {
            return Drivers.Find(d => d.CarIndex == CarIdx);
        }

        public int GetStrengthOfField()
        {
            int sof = 0;
            foreach (Driver d in Drivers)
                sof += d.IRating;

            return sof / Drivers.Count;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            int newIndex = (int)api.GetData("CamCarIdx");
            if (newIndex != CamCarIndex)
            {
                CamCarIndex = newIndex;
                window.UpdateSelectedDriver(CamCarIndex.ToString());
            }

            List<Dictionary<string, object>> driverMapList = rootNode.GetMapList("DriverInfo.Drivers");
            bool added = false;
            foreach (Dictionary<string, object> dict in driverMapList)
            {
                object carIndex = null;
                if (dict.TryGetValue("CarIdx", out carIndex) && carIndex is string)
                {
                    int carIdx = int.Parse((string)carIndex);
                    if (!driverExists(carIdx))
                    {
                        added = true;
                        Driver driver = parseDriver(dict);
                        if (driver != null)
                            Drivers.Add(driver);
                    }
                }
            }

            if (added)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TMTVO.Controller.TMTVO.Instance.iRControls.UpdateDrivers();
                }));
        }

        private bool driverExists(int carIdx)
        {
            return Drivers.Find(d => d.CarIndex == carIdx) != null;
        }

        private Driver parseDriver(Dictionary<string, object> dict)
        {
            int spec = int.Parse((string)dict.GetDictValue("IsSpectator"));
            if (spec != 0)
                return null;

            Driver driver = new Driver();
            foreach (KeyValuePair<string, object> kv in dict)
            {
                switch (kv.Key)
                {
                    case "CarIdx":
                        driver.CarIndex = int.Parse((string)kv.Value);
                        break;
                    case "UserName":
                        driver.FullName = (string)kv.Value;
                        break;
                    case "Initials":
                        driver.Initials = (string)kv.Value;
                        break;
                    case "UserID":
                        driver.UserId = int.Parse((string)kv.Value);
                        break;
                    case "CarNumber":
                        driver.Car.CarNumber = ((string)kv.Value).Substring(1, ((string)kv.Value).Length - 2);
                        break;
                    case "CarPath":
                        driver.Car.CarName = (string)kv.Value;
                        break;
                    case "CarClassID":
                        driver.Car.CarClassId = int.Parse((string)kv.Value);
                        break;
                    case "CarID":
                        driver.Car.CarId = int.Parse((string)kv.Value);
                        break;
                    case "CarClassShortName":
                        driver.Car.CarClassShortName = (string)kv.Value;
                        break;
                    case "CarClassRelSpeed":
                        driver.Car.CarClassRelSpeed = int.Parse((string)kv.Value);
                        break;
                    case "CarClassLicenseLevel":
                        driver.Car.CarClassLicenceLevel = int.Parse((string)kv.Value);
                        break;
                    case "IRating":
                        driver.IRating = int.Parse((string)kv.Value);
                        break;
                    case "LicColor":
                        driver.LicColor = (Color)ColorConverter.ConvertFromString("#FF" + kv.Value.ToString().Substring(2));
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
                                driver.Licence = LicenceLevel.R;
                                break;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                driver.Licence = LicenceLevel.D;
                                break;
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                driver.Licence = LicenceLevel.C;
                                break;
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                                driver.Licence = LicenceLevel.B;
                                break;
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                                driver.Licence = LicenceLevel.A;
                                break;
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                                driver.Licence = LicenceLevel.P;
                                break;
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                                driver.Licence = LicenceLevel.WC;
                                break;
                            default:
                                driver.Licence = LicenceLevel.None;
                                break;
                        }
                        break;
                    case "LicSubLevel":
                        driver.SafetyRating = int.Parse((string)kv.Value);
                        break;
                    case "ClubName":
                        driver.ClubName = (string)kv.Value;
                        break;
                    case "DivisionName":
                        driver.Division = (string)kv.Value;
                        break;
                }

            }

            return driver;
        }

        public override void Reset()
        {
            Drivers.Clear();
        }
    }
}
