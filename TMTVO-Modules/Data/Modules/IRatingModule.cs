using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Modules;
using Yaml;

namespace TMTVO_Modules.Data.Modules
{
    public class IRatingModule : Module
    {
        private static double log = 1600 / Math.Log(2);

        private LiveStandingsModule liveStandings;
        private DriverModule drivers;

        private int lastDriversCount;

        private double[,] matrix;
        private double[] exponentialSoF;
        private double[] expectedScores;
        private double[] fudgeFactors;

        private List<double> changeStarters;
        private List<double> expChangeNonStarters;

        public IRatingModule() : base("iRatingModule")
        {
            lastDriversCount = -1;
            changeStarters = new List<double>();
            expChangeNonStarters = new List<double>();
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            SessionType type = (api.FindModule("SessionTimer") as SessionTimerModule).SessionType;
            if (type != SessionType.LapRace && type != SessionType.TimeRace)
                return;

            if (liveStandings == null)
                liveStandings = api.FindModule("LiveStandings") as LiveStandingsModule;

            if (drivers == null)
                drivers = api.FindModule("DriverModule") as DriverModule;

            int driversCount = drivers.DriversCount;
            int notStarters = liveStandings.Items.Count(d => d.Dns);
            if (lastDriversCount != driversCount)
            {
                matrix = new double[driversCount, driversCount + 1];
                exponentialSoF = new double[driversCount];
                expectedScores = new double[driversCount];
                fudgeFactors = new double[driversCount];
            }

            for (int i = 0; i < driversCount; i++)
            {
                LiveStandingsItem driver = liveStandings.FindDriverByPos(i + 1);
                if (driver == null)
                    return;

                matrix[i, 0] = driver.Driver.IRating;
                exponentialSoF[i] = Math.Exp(-driver.Driver.IRating / log);
                fudgeFactors[i] = driver.Dns ? 0 : (((driversCount - (notStarters / 2D)) / 2 - driver.PositionLive) / 100);
            }

            for (int i = 0; i < driversCount; i++)
            {
                for (int j = 1; j <= driversCount; j++)
                {
                    LiveStandingsItem driverSelf = liveStandings.FindDriverByPos(j);
                    LiveStandingsItem driverOpponent = liveStandings.FindDriverByPos(i + 1);
                    if (driverSelf == null || driverOpponent == null)
                        return;

                    matrix[i, j] = calcMatrixEntry(driverOpponent.Driver.IRating, driverSelf.Driver.IRating);
                }
            }

            changeStarters.Clear();
            expChangeNonStarters.Clear();

            foreach (LiveStandingsItem driver in liveStandings.Items)
            {
                int index = driver.PositionLive - 1;
                double expectedScore = -0.5D;
                for (int i = 1; i <= driversCount; i++)
                    expectedScore += matrix[index, i];

                expectedScores[index] = expectedScore;
                if (driver.Dns)
                    expChangeNonStarters.Add(expectedScore);
            }

            foreach (LiveStandingsItem driver in liveStandings.Items)
            {
                if (!driver.Dns)
                {
                    int i = driver.PositionLive - 1;
                    double change = (driversCount - driver.PositionLive - expectedScores[i] - fudgeFactors[i]) * 200D / (driversCount - notStarters);
                    changeStarters.Add(change);

                    driver.IRatingChange = (int)change;
                }
            }

            foreach (LiveStandingsItem driver in liveStandings.Items)
            {
                if (driver.Dns)
                {
                    int i = driver.PositionLive - 1;

                    double sum = 0D;
                    foreach (double d in changeStarters)
                        sum += d;

                    double avg = 0;
                    foreach (double d in expChangeNonStarters)
                        avg += d;

                    avg /= expChangeNonStarters.Count;

                    double change = (-sum) / notStarters * expectedScores[i] / avg;
                    driver.IRatingChange = (int)change;
                }
            }

            lastDriversCount = driversCount;
        }

        public override void Reset()
        {
            matrix = null;
        }

        private double calcMatrixEntry(double iRatingOldD, double iRatingOldO)
        {
            return (1D - Math.Exp(-iRatingOldD / log)) * Math.Exp(-iRatingOldO / log) 
                / ((1D - Math.Exp(-iRatingOldO / log)) * Math.Exp(-iRatingOldD / log) + (1D - Math.Exp(-iRatingOldD / log)) * Math.Exp(-iRatingOldO / log));
        }
    }
}
