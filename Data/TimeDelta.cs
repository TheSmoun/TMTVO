using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class TimeDelta
    {
        private Single splitdistance = 20;
        private Int32 maxcars = 64;
        private Double[][] splits = new Double[0][];
        private Int32[] splitPointer = new Int32[0];
        private Single splitLength;
        private Double prevTimestamp;
        private Int32 followed;
        private Double[] bestlap;
        private Double[] currentlap;
        private Boolean validbestlap;
        private Double lapstarttime;
        private Int32 arraySize;

        public TimeDelta(Single length, Single splitdist, Int32 drivers)
        {
            // save split distance
            this.splitdistance = splitdist;

            // save car count
            maxcars = drivers;
            // split times every 10 meters
            arraySize = (Int32)Math.Round(length / splitdistance);

            // set split length
            splitLength = (Single)(1.0 / (Double)arraySize);

            // init best lap
            followed = -1;
            bestlap = new Double[arraySize];
            currentlap = new Double[arraySize];
            validbestlap = false;

            // initialize array
            splits = new Double[maxcars][];
            splitPointer = new Int32[maxcars];
            for (Int32 i = 0; i < maxcars; i++)
                splits[i] = new Double[arraySize];
        }

        public void SaveBestLap(Int32 caridx)
        {
            followed = caridx;
        }

        public TimeSpan BestLap { get { if (validbestlap) return new TimeSpan(0, 0, 0, (Int32)bestlap[bestlap.Length - 1], (Int32)((bestlap[bestlap.Length - 1] % 1) * 1000)); else return new TimeSpan(); } set { } }

        public void Update(Double timestamp, Single[] trackPosition)
        {
            Double[] temp = Array.ConvertAll(trackPosition, item => (Double)item);
            Update(timestamp, temp);
        }

        public void Update(Double timestamp, Double[] trackPosition)
        {
            // sanity check
            if (timestamp > prevTimestamp)
            {
                Int32 currentSplitPointer;

                for (Int32 i = 0; i < trackPosition.Length; i++)
                {
                    if (trackPosition[i] > 0)
                    {
                        // interpolate split border crossing
                        currentSplitPointer = (Int32)Math.Floor((trackPosition[i] % 1) / splitLength);

                        if (currentSplitPointer != splitPointer[i])
                        {
                            // interpolate
                            Double distance = trackPosition[i] - (currentSplitPointer * splitLength);
                            Double correction = distance / splitLength;
                            Double currentSplitTime = timestamp - ((timestamp - prevTimestamp) * correction);
                            Boolean newlap = false;

                            if (currentSplitPointer < (100 / splitdistance) && splitPointer[i] > arraySize - (100 / splitdistance))
                                newlap = true;

                            // check if we need interpolation over zero values (splithop > 1)
                            Int32 splithop = currentSplitPointer - splitPointer[i];
                            Double splitcumulator = (currentSplitTime - prevTimestamp) / splithop;
                            Int32 k = 1;

                            // check if we crossed the s/f-line (2*10 split threshold, otherwise we miss it)
                            if (splithop < 0 && newlap)
                            {
                                splithop = arraySize - splitPointer[i] + currentSplitPointer;

                                // in case it new best lap precalculate rest of the lap
                                if (followed >= 0 && i == followed)
                                {
                                    for (Int32 j = splitPointer[i] + 1; j < arraySize; j++)
                                    {
                                        splits[i][j % arraySize] = splits[i][splitPointer[i]] + k++ * splitcumulator;
                                    }
                                }
                            }

                            // save in case of new lap record
                            if (followed >= 0 && i == followed)
                            {
                                // check new lap
                                if (newlap)
                                {
                                    if ((currentSplitTime - splits[i][0]) < bestlap[bestlap.Length - 1] || bestlap[bestlap.Length - 1] == 0)
                                    {
                                        validbestlap = true;
                                        // save lap and substract session time offset
                                        for (Int32 j = 0; j < bestlap.Length - 1; j++)
                                        {
                                            bestlap[j] = splits[i][j + 1] - splits[i][0];
                                            if (splits[i][j + 1] == 0.0)
                                                validbestlap = false;
                                        }

                                        bestlap[bestlap.Length - 1] = currentSplitTime - splits[i][0];
                                    }
                                }

                                lapstarttime = currentlap[currentSplitPointer];
                                currentlap[currentSplitPointer] = currentSplitTime;
                            }

                            // fill hopped sectors if necessary
                            if (splithop > 1)
                            {
                                k = 1;
                                for (Int32 j = splitPointer[i] + 1; j % arraySize != currentSplitPointer; j++)
                                {
                                    splits[i][j % arraySize] = splits[i][splitPointer[i]] + (k++ * splitcumulator);
                                }
                            }

                            // save
                            splits[i][currentSplitPointer] = currentSplitTime;
                            splitPointer[i] = currentSplitPointer;
                        }
                    }
                }
                prevTimestamp = timestamp;
            }
        }

        public TimeSpan GetBestLapDelta(Single trackPosition)
        {
            return GetBestLapDelta((Double)trackPosition);
        }

        public TimeSpan GetBestLapDelta(Double trackPosition)
        {
            if (validbestlap)
            {
                Int32 currentSplitPointer = (Int32)Math.Floor((Math.Abs(trackPosition) % 1) / splitLength);
                Double delta;

                if (currentSplitPointer == 0)
                    delta = (splits[followed][0] - lapstarttime) - bestlap[bestlap.Length - 1];
                else if (currentSplitPointer == (bestlap.Length - 1))
                    delta = (splits[followed][currentSplitPointer] - lapstarttime) - bestlap[bestlap.Length - 1];
                else
                    delta = (splits[followed][currentSplitPointer] - splits[followed][bestlap.Length - 1]) - bestlap[currentSplitPointer - 1];

                return new TimeSpan(0, 0, 0, (Int32)Math.Floor(delta), (Int32)Math.Abs((delta % 1) * 1000));
            }
            else
            {
                return new TimeSpan();
            }
        }

        public TimeSpan GetDelta(Int32 caridx1, Int32 caridx2)
        {
            // validate
            if (caridx1 < maxcars && caridx2 < maxcars && caridx1 >= 0 && caridx2 >= 0)
            {
                // comparing latest finished split
                Int32 comparedSplit = splitPointer[caridx1];

                // catch negative index and loop it to last index
                if (comparedSplit < 0)
                    comparedSplit = splits[caridx1].Length - 1;

                Double delta = splits[caridx1][comparedSplit] - splits[caridx2][comparedSplit];

                //Console.WriteLine(prevTimestamp + " " + splits[caridx1][comparedSplit] + " " + splits[caridx2][comparedSplit]);

                if (splits[caridx1][comparedSplit] == 0 || splits[caridx2][comparedSplit] == 0)
                    return new TimeSpan();
                //else if (delta < 0)
                //    return new TimeSpan();
                else
                    return new TimeSpan(0, 0, 0, (Int32)Math.Floor(delta), (Int32)Math.Abs((delta % 1) * 1000));
            }
            else
            {
                return new TimeSpan();
            }
        }

        public void StoreLap(String filename)
        {

            FileStream file = File.Create(filename);
            DeflateStream Compress = new DeflateStream(file, CompressionMode.Compress);

            Byte[] buf;
            for (Int32 i = 0; i < bestlap.Length; i++)
            {
                buf = BitConverter.GetBytes(bestlap[i]);
                Compress.Write(buf, 0, buf.Length);
            }

            Compress.Close();
            file.Close();

        }

        public void LoadLap(String filename)
        {
            if (File.Exists(filename))
            {
                FileStream file = File.OpenRead(filename);
                DeflateStream Compress = new DeflateStream(file, CompressionMode.Decompress);
                Byte[] buf = new Byte[sizeof(Double)];
                Int32 arrPtr = 0;
                Int32 retval = 0;

                do
                {
                    retval = Compress.Read(buf, 0, sizeof(Double));
                    if (arrPtr < bestlap.Length && retval > 0)
                        bestlap[arrPtr++] = BitConverter.ToDouble(buf, 0);
                } while (retval > 0);

                if (arrPtr == bestlap.Length)
                    validbestlap = true;
                else
                {
                    validbestlap = false;
                    bestlap = new Double[arraySize];
                }
                Compress.Close();
                file.Close();
            }
        }
    }
}
