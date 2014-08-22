using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;

namespace TMTVO.Data.Modules
{
    public class LiveStandingsItem : Component
    {
        public Driver Driver { get; private set; }
        public List<Lap> Laps { get; private set; }
        public Lap CurrentLap { get; private set; }
        public Lap PreviousLap { get; private set; }
        public int Position { get; private set; }
        public int OldPosition { get; private set; }
        public int ClassPosition { get; private set; }
        public float FastestLapTime { get; private set; }
        public int FastestLapNumber { get; private set; }
        public float LastLapTime { get; private set; }
        public float GapTime { get; private set; }
        public int GapLaps { get; private set; }
        public int LapsLed { get; private set; }
        public int ClassLapsLed { get; private set; }
        public int LapsComplete { get; private set; }
        public int Incidents { get; private set; }
        public double LapBegin { get; private set; }
        public bool InPits { get; private set; }
        public bool Finished { get; private set; }
        public float Speed { get; private set; }
        public double PrevSpeed { get; private set; }
        public double CurrentTrackPct { get; private set; }
        public double PrevTrackPct { get; private set; }
        public double PrevTrackPctUpdate { get; private set; }
        public int Sector { get; private set; }
        public double SectorBegin { get; private set; }
        public SurfaceType Surface { get; private set; }
        public bool PositionImproved { get; set; }
        public bool PositionLost { get; set; }
        public bool LapTimeImproved { get; set; }

        public float GapLive
        {
            get
            {
                if (Position > 1 && Speed > 1)
                {
                    TimeDelta delta = ((TimeDeltaModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("TimeDelta")).TimeDelta;
                    LiveStandingsModule standings = ((LiveStandingsModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("LiveStandings"));

                    return (float)delta.GetDelta(this.Driver.CarIndex, standings.FindDriverByPos(Position - 1).Driver.CarIndex).TotalSeconds;
                }
                else
                {
                    return 0F;
                }
            }
        }

        public float GapLiveLeader
        {
            get
            {
                if (Position > 1 && Speed > 1)
                {
                    TimeDelta delta = ((TimeDeltaModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("TimeDelta")).TimeDelta;
                    LiveStandingsModule standings = ((LiveStandingsModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("LiveStandings"));

                    return (float)delta.GetDelta(this.Driver.CarIndex, standings.FindDriverByPos(1).Driver.CarIndex).TotalSeconds;
                }
                else
                {
                    return 0F;
                }
            }
        }

        private double prevTime;
        private bool first;

        public LiveStandingsItem(Driver driver)
        {
            this.Driver = driver;
            Laps = new List<Lap>();
            first = true;
            prevTime = float.MaxValue;
            CurrentLap = new Lap();
            PreviousLap = new Lap();
        }

        public void Update(Dictionary<string, object> dict, API api)
        {
            double currTime = (double)api.GetData("SessionTime");
            OldPosition = Position;

            int carIdx = int.Parse(dict.GetDictValue("CarIdx"));
            if (Driver == null || Driver.CarIndex != carIdx)
                return;

            Position = int.Parse(dict.GetDictValue("Position"));
            PositionImproved = Position < OldPosition || (OldPosition == 0 && Position != 0) && !first;
            PositionLost = OldPosition < Position && !first;
            ClassPosition = int.Parse(dict.GetDictValue("ClassPosition")) + 1;
            GapLaps = int.Parse(dict.GetDictValue("Lap"));
            GapTime = float.Parse(dict.GetDictValue("Time").Replace('.', ','));
            FastestLapNumber = int.Parse(dict.GetDictValue("FastestLap"));
            float newTime = float.Parse(dict.GetDictValue("FastestTime").Replace('.', ','));
            LapTimeImproved = newTime < FastestLapTime || (newTime != 0 && FastestLapTime == 0) && !first;
            FastestLapTime = newTime;
            LastLapTime = float.Parse(dict.GetDictValue("LastTime").Replace('.', ','));
            LapsLed = int.Parse(dict.GetDictValue("LapsLed"));
            LapsComplete = int.Parse(dict.GetDictValue("LapsComplete"));
            Incidents = int.Parse(dict.GetDictValue("Incidents"));

            SurfaceType surfaceType = ((SurfaceType[])api.GetData("CarIdxTrackSurface"))[carIdx];
            InPits = (((bool[])api.GetData("CarIdxOnPitRoad"))[carIdx] || surfaceType == SurfaceType.InPitStall || surfaceType == SurfaceType.NotInWorld);
            Surface = surfaceType;

            // TODO Fix this hole thing
            SessionTimerModule sessionTimer = api.FindModule("SessionTimer") as SessionTimerModule;
            SessionsModule sessions = api.FindModule("Sessions") as SessionsModule;
            LiveStandingsModule standings = api.FindModule("LiveStandings") as LiveStandingsModule;

            if (CurrentLap == null)
                CurrentLap = new Lap();

            double timeOffset = 0;
            if ((currTime - (double)api.GetData("ReplaySessionTime")) < 2)
                        timeOffset = ((int)api.GetData("ReplayFrameNum") - currTime * 60);

            double curpos = (double)((float[])api.GetData("CarIdxLapDistPct"))[carIdx];

            double prevpos = PrevTrackPct;
            double prevupdate = PrevTrackPctUpdate;

            CurrentLap.ReplayPos = (int)(((double)api.GetData("SessionTime") * 60) + timeOffset);

            double now = currTime - ((curpos / (1 + curpos - prevpos)) * (currTime - prevTime));
            if (currTime > prevupdate && curpos != prevpos)
            {
                float speed = 0;

                if (curpos < 0.1 && prevpos > 0.9)
                    speed = (float)((((curpos - prevpos) + 1) * (double)sessions.Track.Length) / (currTime - prevupdate));
                else
                    speed = (float)(((curpos - prevpos) * (double)sessions.Track.Length) / (currTime - prevupdate));

                if (Math.Abs(PrevSpeed - speed) < 1 && (curpos - prevpos) >= 0)
                    Speed = speed;

                PrevSpeed = speed;
                PrevTrackPct = curpos;
                PrevTrackPctUpdate = currTime;

                int lapNumber = ((int[])api.GetData("CarIdxLap"))[carIdx];

                if (!Finished && surfaceType != SurfaceType.NotInWorld)
                    CurrentTrackPct = lapNumber + curpos - 1;

                if (curpos < 0.1 && prevpos > 0.9 && !Finished)
                {
                    if (sessionTimer.SessionType != SessionType.LapRace && sessionTimer.SessionType != SessionType.TimeRace)
                        Finished = true;

                    if (surfaceType != SurfaceType.NotInWorld && speed > 0)
                    {

                        Sector sector = new Sector();
                        sector.Number = Sector;
                        sector.Speed = Speed;
                        sector.Time = (float)(now - SectorBegin);
                        sector.Begin = SectorBegin;

                        CurrentLap.Sectors.Add(sector);
                        CurrentLap.Time = (float)(now - LapBegin);
                        CurrentLap.ClassPosition = ClassPosition;
                        if (sessionTimer.SessionType == SessionType.LapRace || sessionTimer.SessionType == SessionType.TimeRace)
                            CurrentLap.Gap = (float)GapLive;
                        else
                            CurrentLap.Gap = CurrentLap.Time - standings.Leader.FastestLapTime;
                        CurrentLap.GapLaps = 0;


                        if (CurrentLap.LapNumber > 0 && Laps.FindIndex(l => l.LapNumber.Equals(CurrentLap.LapNumber)) == -1 &&
                            (sessionTimer.SessionState != SessionState.Gridding || sessionTimer.SessionState != SessionState.Cooldown))
                        {
                            Laps.Add(CurrentLap);
                        }

                        CurrentLap = new Lap();
                        CurrentLap.LapNumber = ((int[])api.GetData("CarIdxLap"))[carIdx];
                        CurrentLap.Gap = PreviousLap.Gap;
                        CurrentLap.GapLaps = PreviousLap.GapLaps;
                        CurrentLap.ReplayPos = (int)(((double)api.GetData("SessionTime") * 60) + timeOffset);
                        CurrentLap.SessionTime = now;
                        SectorBegin = now;
                        Sector = 0;
                        LapBegin = now;

                        if (sessionTimer.SessionFlags == SessionFlag.Yellow && Position == 1)
                            sessionTimer.CautionLaps++;

                        if (ClassPosition == 1 && CurrentLap.LapNumber > 1)
                            ClassLapsLed++;
                    }
                }

                if (sessions.Track.Sectors.Count > 0 && Driver.CarIndex >= 0)
                {
                    for (int j = 0; j < sessions.Track.Sectors.Count; j++)
                    {
                        if (curpos > sessions.Track.Sectors[j] && j > Sector)
                        {
                            double now2 = currTime - ((curpos - sessions.Track.Sectors[j]) * (curpos - prevpos));
                            Sector sector = new Sector();
                            sector.Number = Sector;
                            sector.Time = (float)(now2 - SectorBegin);
                            sector.Speed = Speed;
                            sector.Begin = SectorBegin;
                            CurrentLap.Sectors.Add(sector);
                            SectorBegin = now2;
                            Sector = j;
                        }
                    }
                }

                if (CurrentLap.LapNumber + CurrentLap.GapLaps >= sessionTimer.LapsTotal && surfaceType != SurfaceType.NotInWorld &&
                    (sessionTimer.SessionType == SessionType.LapRace || sessionTimer.SessionType == SessionType.TimeRace) && !Finished)
                {
                    CurrentTrackPct = (Math.Floor(CurrentTrackPct) + 0.0064) - (0.0001 * Position);
                    Finished = true;
                }                  
            }

            CurrentLap.Time = (float)(now - LapBegin);
            prevTime = currTime;
            // TODO Fix up to here
            first = false;
        }

        public string FastestLapTimeSting
        {
            get
            {
                int min = (int)(FastestLapTime / 60);
                float sectime = FastestLapTime % 60;
                StringBuilder sb = new StringBuilder();
                if (min > 0)
                    sb.Append(min).Append(':');

                sb.Append(sectime.ToString("00.000"));
                return sb.ToString().Replace(',', '.');
            }
        }
    }
}
