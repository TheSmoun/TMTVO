using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class TimeDeltaModule : Module
    {
        private int maxCars = 64;
        private float splitDistance = 20;
        private double[][] splits = new double[0][];
        private int[] splitPointer = new int[0];
        private float splitLength;
        private double prevTimestamp;
        private int followed;
        private double[] bestLap;
        private double[] currentLap;
        private bool validBestLap;
        private double lapStartTime;
        private int arraySize;

        public TimeDeltaModule(float length, float splitDist, int drivers) : base("TimeDelta")
        {
            this.splitDistance = splitDist;
            maxCars = drivers;
            arraySize = (int)Math.Round(length / splitDistance);
            splitLength = (float)(1.0 / (double)arraySize);

            followed = -1;
            bestLap = new double[arraySize];
            currentLap = new double[arraySize];
            validBestLap = false;

            splits = new double[maxCars][];
            splitPointer = new int[maxCars];
            for (int i = 0; i < maxCars; i++)
                splits[i] = new double[arraySize];
        }

        public void SaveBestLap(int caridx)
        {
            followed = caridx;
        }

        public TimeSpan BestLap
        {
            get
            {
                if (validBestLap)
                    return new TimeSpan(0, 0, 0, (int)bestLap[bestLap.Length - 1], (int)((bestLap[bestLap.Length - 1] % 1) * 1000));
                else
                    return new TimeSpan();
            }
        }

        public void Update(double timestamp, float[] trackPosition)
        {
            double[] temp = Array.ConvertAll(trackPosition, item => (double)item);
            Update(timestamp, temp);
        }

        public void Update(double timestamp, double[] trackPosition)
        {
            if (timestamp > prevTimestamp)
            {
                int currentSplitPointer;
                for (int i = 0; i < trackPosition.Length; i++)
                {
                    if (trackPosition[i] > 0)
                    {
                        currentSplitPointer = (int)Math.Floor((trackPosition[i] % 1) / splitLength);

                        if (currentSplitPointer != splitPointer[i])
                        {
                            double distance = trackPosition[i] - (currentSplitPointer * splitLength);
                            double correction = distance / splitLength;
                            double currentSplitTime = timestamp - ((timestamp - prevTimestamp) * correction);
                            bool newlap = false;

                            if (currentSplitPointer < (100 / splitDistance) && splitPointer[i] > arraySize - (100 / splitDistance))
                                newlap = true;

                            int splithop = currentSplitPointer - splitPointer[i];
                            double splitcumulator = (currentSplitTime - prevTimestamp) / splithop;
                            int k = 1;

                            if (splithop < 0 && newlap)
                            {
                                splithop = arraySize - splitPointer[i] + currentSplitPointer;
                                if (followed >= 0 && i == followed)
                                {
                                    for (int j = splitPointer[i] + 1; j < arraySize; j++)
                                    {
                                        splits[i][j % arraySize] = splits[i][splitPointer[i]] + k++ * splitcumulator;
                                    }
                                }
                            }

                            if (followed >= 0 && i == followed)
                            {
                                if (newlap)
                                {
                                    if ((currentSplitTime - splits[i][0]) < bestLap[bestLap.Length - 1] || bestLap[bestLap.Length - 1] == 0)
                                    {
                                        validBestLap = true;
                                        for (int j = 0; j < bestLap.Length - 1; j++)
                                        {
                                            bestLap[j] = splits[i][j + 1] - splits[i][0];
                                            if (splits[i][j + 1] == 0.0)
                                                validBestLap = false;
                                        }

                                        bestLap[bestLap.Length - 1] = currentSplitTime - splits[i][0];
                                    }
                                }

                                lapStartTime = currentLap[currentSplitPointer];
                                currentLap[currentSplitPointer] = currentSplitTime;
                            }

                            if (splithop > 1)
                            {
                                k = 1;
                                for (int j = splitPointer[i] + 1; j % arraySize != currentSplitPointer; j++)
                                {
                                    splits[i][j % arraySize] = splits[i][splitPointer[i]] + (k++ * splitcumulator);
                                }
                            }

                            splits[i][currentSplitPointer] = currentSplitTime;
                            splitPointer[i] = currentSplitPointer;
                        }
                    }
                }
                prevTimestamp = timestamp;
            }
        }

        public TimeSpan GetBestLapDelta(float trackPosition)
        {
            return GetBestLapDelta((double)trackPosition);
        }

        public TimeSpan GetBestLapDelta(double trackPosition)
        {
            if (validBestLap)
            {
                int currentSplitPointer = (int)Math.Floor((Math.Abs(trackPosition) % 1) / splitLength);
                double delta;

                if (currentSplitPointer == 0)
                    delta = (splits[followed][0] - lapStartTime) - bestLap[bestLap.Length - 1];
                else if (currentSplitPointer == (bestLap.Length - 1))
                    delta = (splits[followed][currentSplitPointer] - lapStartTime) - bestLap[bestLap.Length - 1];
                else
                    delta = (splits[followed][currentSplitPointer] - splits[followed][bestLap.Length - 1]) - bestLap[currentSplitPointer - 1];

                return new TimeSpan(0, 0, 0, (int)Math.Floor(delta), (int)Math.Abs((delta % 1) * 1000));
            }
            else
            {
                return new TimeSpan();
            }
        }

        public TimeSpan GetDelta(int caridx1, int caridx2)
        {
            if (caridx1 < maxCars && caridx2 < maxCars && caridx1 >= 0 && caridx2 >= 0)
            {
                int comparedSplit = splitPointer[caridx1];
                if (comparedSplit < 0)
                    comparedSplit = splits[caridx1].Length - 1;

                double delta = splits[caridx1][comparedSplit] - splits[caridx2][comparedSplit];
                if (splits[caridx1][comparedSplit] == 0 || splits[caridx2][comparedSplit] == 0)
                    return new TimeSpan();
                else
                    return new TimeSpan(0, 0, 0, (int)Math.Floor(delta), (int)Math.Abs((delta % 1) * 1000));
            }
            else
            {
                return new TimeSpan();
            }
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            // TODO implement
        }
    }
}
