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
        public float SpeedKmh
        {
            get
            {
                return Speed * 3.6F;
            }
        }
        public float SpeedMph
        {
            get
            {
                return Speed * 2.23693629F;
            }
        }
        public double PrevSpeed { get; private set; }
        public double CurrentTrackPct { get; private set; }
        public double PrevTrackPct { get; private set; }
        public double PrevTrackPctUpdate { get; private set; }
        public int Sector { get; private set; }
        public double SectorBegin { get; private set; }
        public SurfaceType Surface { get; private set; }
        public bool PositionImprovedBattleFor { get; set; }
        public bool PositionLostBattleFor { get; set; }
        public bool PositionImprovedRaceBar { get; set; }
        public bool PositionLostRaceBar { get; set; }
        public bool PositionImprovedTiming { get; set; }
        public bool PositionLostTiming { get; set; }
        public bool LapTimeImproved { get; set; }
        public int PositionImprovements
        {
            get
            {
                return GridModule.FindDriverStatic(this).Position - PositionLive;
            }
        }

        private int positionLive;

        public float GapLive
        {
            get
            {
                if (PositionLive > 1 && Speed > 1)
                {
                    TimeDelta delta = ((TimeDeltaModule)API.Instance.FindModule("TimeDelta")).TimeDelta;
                    LiveStandingsModule standings = ((LiveStandingsModule)API.Instance.FindModule("LiveStandings"));

                    return (float)delta.GetDelta(this.Driver.CarIndex, standings.FindDriverByPos(PositionLive - 1).Driver.CarIndex).TotalSeconds;
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
                if (PositionLive > 1 && Speed > 1)
                {
                    TimeDelta delta = ((TimeDeltaModule)API.Instance.FindModule("TimeDelta")).TimeDelta;
                    LiveStandingsModule standings = ((LiveStandingsModule)API.Instance.FindModule("LiveStandings"));

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


        public int PositionLive
        {
            get
            {
                return positionLive;
            }
            set
            {
                if (value == positionLive)
                    return;

                if (value < positionLive)
                {
                    PositionImprovedBattleFor = PositionImprovedRaceBar = PositionImprovedTiming = true;
                    PositionLostBattleFor = PositionLostRaceBar = PositionLostTiming = false;
                }
                else
                {
                    PositionLostBattleFor = PositionLostRaceBar = PositionLostTiming = true;
                    PositionImprovedBattleFor = PositionImprovedRaceBar = PositionImprovedTiming = false;
                }

                positionLive = value;
            }
        }

        private double prevtime = 0;
        private double currentime = 0;
        private float Prevspeed;
        public double CurrentSessionTime { get; set; }
        public double Begin { get; set; }

        public void Update(Dictionary<string, object> dict, API api, Module caller)
        {
            double currTime = (double)api.GetData("SessionTime");
            OldPosition = Position;

            int carIdx = int.Parse(dict.GetDictValue("CarIdx"));
            if (Driver == null || Driver.CarIndex != carIdx)
                return;

            Position = int.Parse(dict.GetDictValue("Position"));
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

            Track track = ((SessionsModule)API.Instance.FindModule("Sessions")).Track;

            SessionTimerModule m = (SessionTimerModule)API.Instance.FindModule("SessionTimer");
            SessionType sessionType = m.SessionType;
            SessionState sessionState = m.SessionState;
            int finishLine = m.LapsTotal + 1;
            if (finishLine < 0)
                finishLine = int.MaxValue;

            SurfaceType surface = (SurfaceType)((int[])api.GetData("CarIdxTrackSurface"))[Driver.CarIndex];
            int lapNumber = ((int[])api.GetData("CarIdxLap"))[Driver.CarIndex];
            float trackPct = ((float[])api.GetData("CarIdxLapDistPct"))[Driver.CarIndex];

            if (currentime >= prevtime)
                currentime = (double)api.GetData("SessionTime");

            double timeoffset = 0;
            if (((double)api.GetData("SessionTime") - (double)api.GetData("ReplaySessionTime")) < 2)
                timeoffset = (int)api.GetData("ReplayFrameNum") - ((double)api.GetData("SessionTime") * 60);

            double prevpos = PrevTrackPct;
            double prevupdate = PrevTrackPctUpdate;
            float curpos = ((float[])api.GetData("CarIdxLapDistPct"))[Driver.CarIndex];

            CurrentLap.ReplayPos = (int)(((double)api.GetData("SessionTime") * 60) + timeoffset);

            if (currentime > prevupdate && curpos != prevpos)
            {
                float speed = 0;

                if (curpos < 0.1 && prevpos > 0.9)
                    speed = (float)((((curpos - prevpos) + 1) * (double)track.Length) / (currentime - prevupdate));
                else
                    speed = (float)(((curpos - prevpos) * (double)track.Length) / (currentime - prevupdate));

                if (Math.Abs(Prevspeed - speed) < 1 && (curpos - prevpos) >= 0)
                    Speed = speed;

                Prevspeed = speed;
                PrevTrackPct = curpos;
                PrevTrackPctUpdate = currentime;

                if (!Finished)
                    CurrentTrackPct = lapNumber + trackPct - 1;

                if (curpos < 0.1 && prevpos > 0.9 && !Finished)
                {
                    if (surface != SurfaceType.NotInWorld && speed > 0)
                    {
                        Double now = currentime - ((curpos / (1 + curpos - prevpos)) * (currentime - prevtime));

                        Sector sector = new Sector();
                        sector.Number = Sector;
                        sector.Speed = Speed;
                        sector.Time = (float)(now - SectorBegin);
                        sector.Begin = SectorBegin;

                        CurrentLap.Sectors.Add(sector);
                        CurrentLap.Time = (float)(now - Begin);
                        // TODO CurrentLap.ClassPosition = SharedData.Sessions.CurrentSession.getClassPosition(driver.Driver);
                        if (sessionType == SessionType.LapRace || sessionType == SessionType.TimeRace)
                            CurrentLap.Gap = (float)GapLive;
                        else
                        {
                            LiveStandingsItem leader = ((LiveStandingsModule)caller).Leader;
                            if (leader != null)
                                CurrentLap.Gap = CurrentLap.Time - leader.FastestLapTime;
                        }

                        CurrentLap.GapLaps = 0;


                        if (CurrentLap.LapNumber > 0 && Laps.FindIndex(l => l.LapNumber.Equals(CurrentLap.LapNumber)) == -1 &&
                            (sessionState != SessionState.Gridding || sessionState != SessionState.Cooldown))
                            Laps.Add(CurrentLap);

                        CurrentLap = new Lap();
                        CurrentLap.LapNumber = lapNumber;
                        CurrentLap.Gap = PreviousLap.Gap;
                        CurrentLap.GapLaps = PreviousLap.GapLaps;
                        CurrentLap.ReplayPos = (int)(((double)api.GetData("SessionTime") * 60) + timeoffset);
                        CurrentLap.SessionTime = (double)api.GetData("SessionTime");
                        SectorBegin = now;
                        Sector = 0;
                        Begin = now;

                        // caution lap calc
                        //if (m.SessionFlags.FlagSet(SessionFlag.Yellow) && Position == 1)
                        //    SharedData.Sessions.CurrentSession.CautionLaps++;

                        // class laps led
                        //if (SharedData.Sessions.CurrentSession.getClassLeader(driver.Driver.CarClassName).Driver.CarIdx == driver.Driver.CarIdx && driver.CurrentLap.LapNum > 1)
                        //    driver.ClassLapsLed = driver.ClassLapsLed + 1;
                    }
                }

                if (track.Sectors.Count > 0 && Driver.CarIndex >= 0)
                {
                    for (int j = 0; j < track.Sectors.Count; j++)
                    {
                        if (curpos > track.Sectors[j] && j > Sector)
                        {
                            double now = currentime - ((curpos - track.Sectors[j]) * (curpos - prevpos));
                            Sector sector = new Sector();
                            sector.Number = Sector;
                            sector.Time = (float)(now - SectorBegin);
                            sector.Speed = Speed;
                            sector.Begin = SectorBegin;
                            CurrentLap.Sectors.Add(sector);
                            SectorBegin = now;
                            Sector = j;
                        }
                    }
                }

                if (CurrentLap.LapNumber + CurrentLap.GapLaps >= finishLine && surface != SurfaceType.NotInWorld &&
                    (sessionType == SessionType.LapRace || sessionType == SessionType.TimeRace) && !Finished)
                {
                    ((LiveStandingsModule)caller).UpdateLivePositions();
                    CurrentTrackPct = (Math.Floor(CurrentTrackPct) + 0.0064) - (0.0001 * Position);
                    Finished = true;
                }

                if (Driver.CarIndex >= 0)
                    Surface = surface;
            }

            prevtime = currentime;
            CurrentSessionTime = currentime;
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

        public Lap FastestLap
        {
            get
            {
                Lap l = null;
                for (int i = 0; i < Laps.Count; i++)
                    if (l == null || Laps[i].Time < l.Time)
                        l = Laps[i];

                return l;
            }
        }
    }
}
