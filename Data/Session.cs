using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Session : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            if (this == DataManager.Sessions.CurrentSession)
                DataManager.NotifyPropertyChanged(name);
        }

        public int SessionNumber { get; set; }
        public int SessionLaps { get; set; }
        public double SessionTime { get; set; }
        int lapsComplete;
        public int LeadChanges { get; set; }
        public int Cautions { get; set; }
        public int CautionLaps { get; set; }

        float fastestlap = 0;
        public Driver FastestLapDriver { get; set; }
        public int FastestLapNum { get; set; }

        double time;
        public double TimeRemaining { get; set; }
        public double SessionStartTime { get; set; }
        int sessionstartpos;
        public int FinishLine { get; set; }

        public SessionType SessionType { get; set; }
        public SessionState State { get; set; }
        public SessionFlags Flag { get; set; }
        public SessionStartLights StartLights { get; set; }

        public ResultItem FollowedDriver { get; private set; }
        public ObservableCollection<ResultItem> Standings { get; set; }
        public float PreviousFastestLap { get; set; }
        private Boolean _PitOccupied = false;

        public Session()
        {
            Standings = new ObservableCollection<ResultItem>();
            State = SessionState.Invalid;
        }

        public ResultItem FindDriver(int caridx)
        {
            int index = Standings.IndexOf(Standings.Where(s => s.Driver.CarIndex.Equals(caridx)).FirstOrDefault());
            if (index >= 0)
            {
                return Standings[index];
            }
            else
            {
                return new ResultItem();
            }
        }

        public int Id
        {
            get
            {
                return SessionNumber;
            }
            set
            {
                SessionNumber = value;
                this.NotifyPropertyChanged("Id");
            }
        }

        public int LapsTotal
        {
            get
            {
                if (SessionLaps >= Int32.MaxValue) return 0;
                else return SessionLaps;
            }
            set
            {
                SessionLaps = value;
                this.NotifyPropertyChanged("LapsTotal");
                this.NotifyPropertyChanged("LapsRemaining");
            }
        }

        public int LapsComplete
        {
            get
            {
                if (lapsComplete < 0) return 0;
                else return lapsComplete;
            }
            set
            {
                lapsComplete = value;
                this.NotifyPropertyChanged("LapsComplete");
                this.NotifyPropertyChanged("LapsRemaining");
            }
        }

        public int LapsRemaining
        {
            get
            {
                if ((SessionLaps - lapsComplete) < 0)
                    return 0;
                else
                    return (SessionLaps - lapsComplete);
            }
        }

        public float FastestLap
        {
            get
            {
                return fastestlap;
            }
            set
            {
                PreviousFastestLap = fastestlap;
                fastestlap = value;
                if (SessionType == Data.SessionType.Race)
                    this.NotifyPropertyChanged("FastestLap");
            }
        }

        public int CurrentReplayPosition
        {
            get
            {
                return (int)((time - SessionStartTime) * 60) + sessionstartpos;
            }
            set
            {
                sessionstartpos = value;
            }
        }

        public Boolean PitOccupied
        {
            get { return _PitOccupied; }
            set
            {
                if (_PitOccupied == value)
                    return;
                if (_PitOccupied == false) // Someone entered the pit
                {
                    _PitOccupied = value;
                    DataManager.Triggers.Push(TriggerType.PitOccupied);
                    return;
                }
                // Last ar left the Pits
                _PitOccupied = value;
                DataManager.Triggers.Push(TriggerType.PitEmpty);
            }
        }

        public void CheckPitStatus()
        {
            if (SessionType != SessionType.Race)
            {
                PitOccupied = false;
                return;
            }

            int ct = Standings.Count(s => s.TrackSurface == SurfaceType.InPitStall);
            PitOccupied = (ct > 0);
        }

        public void SetFollowedDriver(int carIdx)
        {
            if ((FollowedDriver == null) || (carIdx != FollowedDriver.Driver.CarIndex))
            {
                FollowedDriver.IsFollowedDriver = false;
                FollowedDriver = FindDriver(carIdx);
                if (FollowedDriver.Driver.CarIndex == carIdx)
                {
                    FollowedDriver.IsFollowedDriver = true;
                    NotifyPropertyChanged("FollowedDriver");
                }
            }
        }

        public ResultItem GetLeader()
        {
            ResultItem stand = this.FindPosition(1, DataOrders.Position);
            if (stand.Driver.CarIndex >= 0)
            {
                return stand;
            }
            else
            {
                return new ResultItem();
            }
        }

        public ResultItem GetLiveLeader()
        {
            ResultItem stand = this.FindPosition(1, DataOrders.LivePosition);
            if (stand.Driver.CarIndex >= 0)
            {
                return stand;
            }
            else
            {
                return new ResultItem();
            }
        }

        public ResultItem FindPosition(int pos, DataOrders order)
        {
            return this.FindPosition(pos, order, null);
        }

        public ResultItem FindPosition(int pos, DataOrders order, string classname)
        {
            int index = -1;
            int i = 1;
            IEnumerable<ResultItem> query;
            switch (order)
            {
                case DataOrders.FastestLap:
                    int lastpos = DataManager.Drivers.Count;

                    if (classname == null)
                        query = DataManager.Sessions.CurrentSession.Standings.OrderBy(s => s.FastestLap);
                    else
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).OrderBy(s => s.FastestLap);
                    foreach (ResultItem si in query)
                    {
                        if (si.FastestLap > 0)
                        {
                            if (i == pos)
                            {
                                index = Standings.IndexOf(Standings.Where(f => f.Driver.CarIndex.Equals(si.Driver.CarIndex)).FirstOrDefault());
                                break;
                            }

                            i++;
                        }
                    }

                    // if not found then driver has no finished laps
                    if (index < 0)
                    {
                        if (classname == null)
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.FastestLap <= 0);
                        else
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).Where(s => s.FastestLap <= 0);

                        foreach (ResultItem si in query)
                        {
                            if (i == pos)
                            {
                                index = Standings.IndexOf(Standings.Where(f => f.Driver.CarIndex.Equals(si.Driver.CarIndex)).FirstOrDefault());
                                break;
                            }

                            i++;
                        }
                    }
                    break;
                case DataOrders.PreviousLap:

                    if (classname == null)
                        query = DataManager.Sessions.CurrentSession.Standings.OrderBy(s => s.PreviousLap.Time);
                    else
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).OrderBy(s => s.PreviousLap.Time);
                    try
                    {
                        foreach (ResultItem si in query)
                        {
                            if (si.PreviousLap.Time > 0)
                            {
                                if (i == pos)
                                {
                                    index = Standings.IndexOf(Standings.Where(f => f.Driver.CarIndex.Equals(si.Driver.CarIndex)).FirstOrDefault());
                                    break;
                                }

                                i++;
                            }
                        }
                    }
                    catch
                    {
                        index = -1;
                    }

                    // if not found then driver has no finished laps
                    if (index < 0)
                    {
                        if (classname == null)
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.PreviousLap.Time <= 0);
                        else
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).Where(s => s.PreviousLap.Time <= 0);

                        foreach (ResultItem si in query)
                        {
                            if (i == pos)
                            {
                                index = Standings.IndexOf(Standings.Where(f => f.Driver.CarIndex.Equals(si.Driver.CarIndex)).FirstOrDefault());
                                break;
                            }
                            i++;
                        }
                    }
                    break;
                case DataOrders.ClassPosition:
                    query = DataManager.Sessions.CurrentSession.Standings.OrderBy(s => s.Driver.Car.CarClassOrder + s.Position).Skip(pos - 1);
                    if (query.Count() > 0)
                    {
                        ResultItem si = query.First();
                        return si;
                    }
                    else
                        return new ResultItem();
                case DataOrders.Points:
                    //query = SharedData.Sessions.CurrentSession.Standings.OrderByDescending(s => Convert.ToInt32(s.Driver.ExternalData[SharedData.theme.pointscol])).Skip(pos - 1);
                    //if (query.Count() > 0)
                    //{
                    //    StandingsItem si = query.First();
                    //    return si;
                    //}
                    //else
                    return new ResultItem();
                case DataOrders.LivePosition:
                    if (classname == null)
                        index = Standings.IndexOf(Standings.Where(f => f.PositionLive.Equals(pos)).FirstOrDefault());
                    else
                    {
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).OrderBy(s => s.PositionLive).Skip(pos - 1);
                        if (query.Count() > 0)
                        {
                            ResultItem si = query.First();
                            return si;
                        }
                        else
                            return new ResultItem();
                    }
                    break;
                case DataOrders.TrackPosition:
                    if (pos < 0)
                    { // infront
                        int skip = (-pos) - 1;
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.DistanceToFollowed > 0 && s.TrackSurface != SurfaceType.NotInWorld).OrderBy(s => s.DistanceToFollowed);
                        if (query.Count() <= skip)
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.DistanceToFollowed < 0 && s.TrackSurface != SurfaceType.NotInWorld).OrderBy(s => s.DistanceToFollowed).Skip((-pos) - 1 - query.Count());
                        else
                            query = query.Skip(skip);
                    }
                    else if (pos > 0)
                    { // behind
                        int skip = pos - 1;
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.DistanceToFollowed < 0 && s.TrackSurface != SurfaceType.NotInWorld).OrderByDescending(s => s.DistanceToFollowed);
                        if (query.Count() <= skip)
                            query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.DistanceToFollowed > 0 && s.TrackSurface != SurfaceType.NotInWorld).OrderByDescending(s => s.DistanceToFollowed).Skip(pos - 1 - query.Count());
                        else
                            query = query.Skip(skip);
                    }
                    else // me
                        return DataManager.Sessions.CurrentSession.FollowedDriver;

                    if (query.Count() > 0)
                    {
                        ResultItem si = query.First();
                        return si;
                    }
                    else
                        return new ResultItem();
                default:
                    if (classname == null)
                        index = Standings.IndexOf(Standings.Where(f => f.Position.Equals(pos)).FirstOrDefault());
                    else
                    {
                        query = DataManager.Sessions.CurrentSession.Standings.Where(s => s.Driver.Car.CarClassName == classname).OrderBy(s => s.Position).Skip(pos - 1);
                        if (query.Count() > 0)
                        {
                            ResultItem si = query.First();
                            return si;
                        }
                        else
                            return new ResultItem();
                    }
                    break;
            }

            if (index >= 0)
                return Standings[index];
            else
                return new ResultItem();
        }

        public int GetClassPosition(Driver driver)
        {
            IEnumerable<ResultItem> query = this.Standings.Where(s => s.Driver.Car.CarClassName == driver.Car.CarClassName).OrderBy(s => s.Position);
            Int32 position = 1;
            foreach (ResultItem si in query)
            {
                if (si.Driver.CarIndex == driver.CarIndex)
                    return position;
                else
                    position++;
            }
            return 0;
        }

        public int GetClassLivePosition(Driver driver)
        {
            IEnumerable<ResultItem> query = this.Standings.Where(s => s.Driver.Car.CarClassName == driver.Car.CarClassName).OrderBy(s => s.PositionLive);
            Int32 position = 1;
            foreach (ResultItem si in query)
            {
                if (si.Driver.CarIndex == driver.CarIndex)
                    return position;
                else
                    position++;
            }
            return 0;
        }

        public ResultItem GetClassLeader(string className)
        {
            if (className.Length > 0)
            {
                IEnumerable<ResultItem> query = this.Standings.Where(s => s.Driver.Car.CarClassName == className).OrderBy(s => s.Position);
                if (query.Count() > 0)
                {
                    ResultItem si = query.First();
                    return si;
                }
                else
                    return new ResultItem();
            }
            else
                return new ResultItem();
        }

        public Int32 getClassCarCount(string className)
        {
            IEnumerable<ResultItem> query = this.Standings.Where(s => s.Driver.Car.CarClassName == className);
            return query.Count();
        }

        public void UpdatePosition()
        {
            int i = 1;
            IEnumerable<ResultItem> query;
            if (this.SessionType == SessionType.Race)
            {
                query = Standings.OrderByDescending(s => s.CurrentTrackPct);
                foreach (ResultItem si in query)
                {
                    si.PositionLive = i++;
                    si.NotifyPosition();
                }
            }
            else
            {

                query = Standings.OrderBy(s => s.Position);
                foreach (ResultItem si in query)
                {
                    si.PositionLive = si.Position;
                    si.NotifyPosition();
                }

            }

            CheckPitStatus();
        }
    }
}
