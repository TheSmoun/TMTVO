using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class DriverModule : Module
    {
        public List<Driver> Drivers { get; private set; }

        public DriverModule() : base("DriverModule")
        {
            Drivers = new List<Driver>();
        }

        public Driver FindDriver(int CarIdx)
        {
            return Drivers.Find(d => d.CarIndex == CarIdx);
        }

        public int SoF
        {
            get
            {
                return (int)SoFDouble;
            }
        }

        public double SoFDouble
        {
            get
            {
                double log = 1600 / Math.Log(2);
                int count = DriversCount;

                double sum = 0;
                foreach (Driver driver in Drivers)
                {
                    if (driver.FullName.StartsWith("Pace Car"))
                        continue;

                    sum += Math.Exp(-driver.IRating / log);
                }

                return log * Math.Log(count / sum);
            }
        }

        public int DriversCount
        {
            get
            {
                List<Driver> query = Drivers.OrderBy(d => d.IRating).ToList();
                if (query.Count > 0 && query.First().IRating == 0 && query.First().FullName.StartsWith("Pace Car"))
                    query.RemoveAt(0);

                return query.Count;
            }
        }

        public IEnumerable<Driver> OrderDriversByNumberPlate()
        {
            return Drivers.OrderBy(d => d.NumberPlateInt);
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            List<Dictionary<string, object>> driverMapList = rootNode.GetMapList("DriverInfo.Drivers");
            foreach (Dictionary<string, object> dict in driverMapList)
            {
                object carIndex = null;
                if (dict.TryGetValue("CarIdx", out carIndex) && carIndex is string)
                {
                    int carIdx = int.Parse((string)carIndex);

                    Driver driver = Drivers.Find(d => d.CarIndex == carIdx);
                    if (driver == null)
                    {
                        object sspec = dict.GetDictValue("IsSpectator");
                        if (sspec == null)
                            continue;

                        int spec = int.Parse((string)sspec);
                        if (spec != 0)
                            continue;

                        object name = dict.GetDictValue("UserName");
                        if (((string)name).StartsWith("Pace Car"))
                            continue;

                        driver = new Driver();
                        Drivers.Add(driver);
                        LiveStandingsModule m = API.Instance.FindModule("LiveStandings") as LiveStandingsModule;
                        if (!m.ContainsItem(carIdx))
                            m.Items.Add(new LiveStandingsItem(driver));
                    }

                    driver.Update(dict, api);
                }
            }
        }

        public override void Reset()
        {
            Drivers.Clear();
        }
    }
}
