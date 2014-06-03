using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class ResultItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            DataManager.NotifyPropertyChanged(name);
        }

        public Driver Driver { get; set; }
        public List<Lap> Laps { get; set; }
        public double PrevSpeed { get; set; }
        public int Position { get; set; }
        public int Sector { get; set; }
        public double SectorBegin { get; set; }
        public int PitStops { get; set; }
        public float PitStopTime { get; set; }
        public DateTime PitStopBegin { get; set; }
        public double Begin { get; set; }
        public bool Finished { get; set; }
        public double OffTrackSince { get; set; }
        public int PositionLive { get; set; }
        public int LapsLed { get; set; }
        public double PrevTrackPct { get; set; }
        public double PrevTrackPctUpdate { get; set; }
        public int ClassLapsLed { get; set; }
        public Stopwatch Stopwatch { get; set; }

        private float speed;
        private float fastestlap;
        private double trackpct;
        private bool isFollowedDriver;
        private int airTimeCount;
        private Lap currentlap;
        private TimeSpan airTimeAirTime;
        public DateTime airTimeLastAirTime;
        private SurfaceType currentTrackSurface;
        private SurfaceType prevTrackSurface;

        public ResultItem()
        {
            airTimeAirTime = TimeSpan.FromMilliseconds(0.0);
            airTimeLastAirTime = DateTime.MinValue;
            Stopwatch = new Stopwatch();
        }

        public void CrossedLine()
        {
            Stopwatch.Restart();
        }

        public Lap FindLap(int num)
        {
            int index = Laps.FindIndex(f => f.LapNumber.Equals(num));
            if (index >= 0)
                return Laps[index];
            else
                return new Lap();
        }

        public float FastestLap
        {
            get
            {
                if (fastestlap != float.MaxValue)
                    return fastestlap;
                else
                    return 0;
            }
            set
            {
                fastestlap = value;
            }
        }

        public string FastestLap_HR
        {
            get
            {
                if (fastestlap != Single.MaxValue)
                    return Utils.floatTime2String(fastestlap, 3, false);
                else
                    return string.Empty;
            }
        }

        public SurfaceType TrackSurface
        {
            get { return currentTrackSurface; }
            set
            {
                prevTrackSurface = currentTrackSurface;
                currentTrackSurface = value;

                // Check if Driver went Off-Road
                if (currentTrackSurface == SurfaceType.OffTrack && prevTrackSurface != SurfaceType.OffTrack)
                {
                    SessionEvent ev = new SessionEvent(SessionEventType.OffTrack, Driver, "Off track", DataManager.Sessions.CurrentSession.SessionType, CurrentLap.LapNumber);
                    DataManager.Events.Add(ev);
                    DataManager.Triggers.Push(new Trigger(Driver.CarIndex, TriggerType.OffTrack));
                }

                if (prevTrackSurface != currentTrackSurface && currentTrackSurface == SurfaceType.NotInWorld)
                {
                    OffTrackSince = DataManager.Sessions.CurrentSession.SessionTime;
                    DataManager.Triggers.Push(new Trigger(Driver.CarIndex, TriggerType.NotInWorld));
                }

                if (prevTrackSurface != currentTrackSurface && currentTrackSurface == SurfaceType.AproachingPits)
                {
                    if (prevTrackSurface == SurfaceType.InPitStall)
                        DataManager.Triggers.Push(new Trigger(Driver.CarIndex, TriggerType.PitOut));
                    else
                        DataManager.Triggers.Push(new Trigger(Driver.CarIndex, TriggerType.PitIn));
                }

                if (DataManager.Sessions.CurrentSession.SessionType == SessionType.Race)
                {
                    // Pit-Stop checks
                    if (currentTrackSurface == SurfaceType.InPitStall)
                    {
                        if (prevTrackSurface != SurfaceType.InPitStall) // Driver entered the pit 
                        {
                            if ((prevTrackSurface != SurfaceType.NotInWorld)) // (not starting from pits!)
                            {
                                if (DataManager.Sessions.CurrentSession.State == SessionState.Racing)
                                {
                                    SessionEvent ev = new SessionEvent(SessionEventType.Pit, Driver, "Pitting on lap " + CurrentLap.LapNumber, DataManager.Sessions.CurrentSession.SessionType, CurrentLap.LapNumber);
                                    DataManager.Events.Add(ev);
                                    PitStops++;
                                }

                                PitStopBegin = DateTime.Now;
                                NotifyPit();
                            }
                        }
                        else
                        {
                            PitStopTime = (float)(DateTime.Now - PitStopBegin).TotalSeconds;
                            NotifyPit();
                        }
                    }
                }

                NotifyPropertyChanged("TrackSurface");
            }
        }

        public TimeSpan AirTimeAirTime
        {
            get
            {
                return airTimeAirTime;
            }
            set
            {
                airTimeAirTime = value;
                NotifyPropertyChanged("AirTimeAirTime");
                NotifyPropertyChanged("AirTimeAirTime_HR");
            }
        }

        public string AirTimeAirTime_HR
        {
            get
            {
                return String.Format("{0:hh\\:mm\\:ss}", airTimeAirTime);
            }
        }

        public DateTime AirTimeLastAirTime
        {
            get
            {
                return airTimeLastAirTime;
            }
        }

        public void AddAirTime(double airTime)
        {
            if (airTime > 0.0)
                AirTimeAirTime = airTimeAirTime.Add(TimeSpan.FromSeconds(airTime));
        }

        public bool IsFollowedDriver
        {
            get { return isFollowedDriver; }
            set
            {
                airTimeLastAirTime = DateTime.Now;
                if (!isFollowedDriver && (value == true))
                {
                    airTimeCount++;
                    NotifyPropertyChanged("AirTimeCount");
                }
                isFollowedDriver = value;
                NotifyPropertyChanged("IsFollowedDriver");
                NotifyPropertyChanged("AirTimeLastAirTime");
            }
        }

        public Lap CurrentLap
        {
            get
            {
                if (currentTrackSurface == SurfaceType.NotInWorld && !Finished)
                    return PreviousLap;
                else
                    return currentlap;
            }
            set { currentlap = value; }
        }

        public double CurrentTrackPct
        {
            get
            {
                if (trackpct > 0)
                    return trackpct;
                else
                    return PreviousLap.LapNumber;
            }
            set
            {
                trackpct = value;
                currentlap.LapNumber = (int)Math.Floor(value);
            }
        }

        public double TrackPct
        {
            get
            {
                return this.trackpct % 1;
            }
        }

        public double DistanceToFollowed
        {
            get
            {
                return (this.trackpct % 1) - DataManager.Sessions.CurrentSession.FollowedDriver.TrackPct;
            }
        }

        public float Speed
        {
            // meters per second
            get
            {
                if (speed > 0)
                    return speed;
                else
                    return 0;
            }
            set
            {
                speed = value;
            }
        }

        public int Speed_kph
        {
            get
            {
                if (speed > 0)
                    return (int)(speed * 3.6);
                else
                    return 0;
            }
        }

        public double IntervalLive
        {
            get
            {
                if (Position > 1 && speed > 1)
                    return DataManager.TimeDelta.GetDelta(this.Driver.CarIndex, DataManager.Sessions.CurrentSession.FindPosition(this.PositionLive - 1, DataOrders.LivePosition).Driver.CarIndex).TotalSeconds;
                else
                    return 0;
            }
        }


        public string IntervalLive_HR_rounded
        {
            get
            {
                return this.IntervalLive_HR(1);
            }
        }

        public string IntervalLive_HR(int rounding)
        {
            if (IntervalLive == 0)
                return "-.--";
            else if ((DataManager.Sessions.CurrentSession.FindPosition(this.PositionLive - 1, DataOrders.LivePosition).CurrentTrackPct - TrackPct) > 1)
                return (DataManager.Sessions.CurrentSession.FindPosition(this.PositionLive - 1, DataOrders.LivePosition).CurrentLap.LapNumber - CurrentLap.LapNumber) + "L";
            else
                return Utils.floatTime2String((float)IntervalLive, rounding, false);
        }

        public string GapLive_HR_rounded
        {
            get
            {
                return this.GapLive_HR(1);
            }
        }

        public string GapLive_HR(int rounding)
        {
            if (GapLive == 0)
                return "-.--";
            else if ((DataManager.Sessions.CurrentSession.GetLiveLeader().CurrentTrackPct - CurrentTrackPct) > 1)
                return (DataManager.Sessions.CurrentSession.GetLiveLeader().CurrentLap.LapNumber - CurrentLap.LapNumber) + "L";
            else
                return Utils.floatTime2String((float)GapLive, rounding, false);
        }

        public string ClassIntervalLive_HR
        {
            get
            {
                if (IntervalLive == 0)
                    return "-.--";
                else if ((DataManager.Sessions.CurrentSession.FindPosition(this.PositionLive - 1, DataOrders.LivePosition, this.Driver.Car.CarClassName).CurrentTrackPct - TrackPct) > 1)
                    return (DataManager.Sessions.CurrentSession.FindPosition(this.PositionLive - 1, DataOrders.LivePosition, this.Driver.Car.CarClassName).CurrentLap.LapNumber - CurrentLap.LapNumber) + "L";
                else
                    return IntervalLive.ToString("0.0");
            }
        }

        public string ClassGapLive_HR
        {
            get
            {
                if (GapLive == 0)
                    return "-.--";
                else if ((DataManager.Sessions.CurrentSession.GetClassLeader(this.Driver.Car.CarClassName).CurrentTrackPct - CurrentTrackPct) > 1)
                    return (DataManager.Sessions.CurrentSession.GetClassLeader(this.Driver.Car.CarClassName).CurrentLap.LapNumber - CurrentLap.LapNumber) + "L";
                else
                    return GapLive.ToString("0.0");
            }
        }

        public double GapLive
        {
            get
            {
                if (this.PositionLive > 1 && this.speed > 1)
                {
                    ResultItem leader = DataManager.Sessions.CurrentSession.GetLiveLeader();
                    return DataManager.TimeDelta.GetDelta(this.Driver.CarIndex, leader.Driver.CarIndex).TotalSeconds;
                }
                else
                    return 0;
            }
        }

        public double IntervalToFollowedLive
        {
            get
            {
                if (this.Driver.CarIndex == DataManager.Sessions.CurrentSession.FollowedDriver.Driver.CarIndex)
                    return 0.0;
                if (this.PositionLive > DataManager.Sessions.CurrentSession.FollowedDriver.PositionLive)
                    return DataManager.TimeDelta.GetDelta(this.Driver.CarIndex, DataManager.Sessions.CurrentSession.FollowedDriver.Driver.CarIndex).TotalSeconds;
                else
                    return DataManager.TimeDelta.GetDelta(DataManager.Sessions.CurrentSession.FollowedDriver.Driver.CarIndex, this.Driver.CarIndex).TotalSeconds;
            }
        }

        public Lap PreviousLap
        {
            get
            {
                if (Finished)
                    return currentlap;
                else
                {
                    int count = (Int32)Math.Floor(trackpct);
                    if (count > 1)
                        if (this.Laps.Exists(l => l.LapNumber.Equals(count)))
                            return this.FindLap(count);
                        else
                            return this.FindLap(count - 1);
                    else if (count == 1 && Laps.Count == 1)
                        return Laps[0];
                    else
                        return new Lap();
                }
            }
        }

        public int HighestPosition
        {
            get
            {
                IEnumerable<Lap> result = Laps.Where(l => l.Position > 0).OrderBy(l => l.Position);
                if (result.Count() > 0)
                    return result.First().Position;
                else
                    return 0;
            }
        }

        public int LowestPosition
        {
            get
            {
                IEnumerable<Lap> result = Laps.OrderByDescending(l => l.Position);
                if (result.Count() > 0)
                    return result.First().Position;
                else
                    return 0;
            }
        }

        public int HighestClassPosition
        {
            get
            {
                IEnumerable<Lap> result = Laps.Where(l => l.ClassPosition > 0).OrderBy(l => l.ClassPosition);
                if (result.Count() > 0)
                    return result.First().ClassPosition;
                else
                    return 0;
            }
        }

        public int LowestClassPosition
        {
            get
            {
                IEnumerable<Lap> result = Laps.OrderByDescending(l => l.ClassPosition);
                if (result.Count() > 0)
                    return result.First().ClassPosition;
                else
                    return 0;
            }
        }

        public void SetDriver(int carIdx)
        {
            int index = DataManager.Drivers.FindIndex(d => d.CarIndex.Equals(carIdx));
            if (index >= 0)
                Driver = DataManager.Drivers[index];
            else
                Driver = new Driver();
        }

        public void NotifyLaps()
        {
            this.NotifyPropertyChanged("Laps");
            this.NotifyPropertyChanged("PreviousLap");
            this.NotifyPropertyChanged("CurrentLap");
        }

        public void NotifySelf()
        {
            this.NotifyPropertyChanged("Driver");
            this.NotifyPropertyChanged("PreviousLap");
            this.NotifyPropertyChanged("FastestLap");
            this.NotifyPropertyChanged("LapsLed");
        }

        public void NotifyPosition()
        {
            this.NotifyPropertyChanged("Speed_kph");
            this.NotifyPropertyChanged("Speed");
            this.NotifyPropertyChanged("IntervalLive_HR_rounded");
            this.NotifyPropertyChanged("GapLive_HR_rounded");
            this.NotifyPropertyChanged("Gap_HR");
            this.NotifyPropertyChanged("Position");
            this.NotifyPropertyChanged("PositionLive");
            this.NotifyPropertyChanged("Sector");
        }

        public void NotifyPit()
        {
            this.NotifyPropertyChanged("PitStops");
            this.NotifyPropertyChanged("PitStopTime");
        }
    }
}
