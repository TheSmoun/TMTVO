using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class iRacingAPI
    {
        private static Dictionary<String, SessionType> sessionTypeMap = new Dictionary<String, SessionType>()
        {
            {"Offline Testing", SessionType.OfflineTesting},
            {"Practice", SessionType.Practice},
            {"Open Qualify", SessionType.Qualifying},
            {"Lone Qualify", SessionType.Qualifying},
            {"Race", SessionType.Race}
        };

        private MainWindow window;
        private iRacingSDK Sdk;
        private bool runApi;
        private int lastUpdate;
        private bool readCache;
        private double currentime = 0;
        private double prevtime = 0;
        private double timeoffset = 0;

        public iRacingAPI(MainWindow window)
        {
            this.window = window;
            this.Sdk = new iRacingSDK();
            runApi = true;
            lastUpdate = -1;

            Thread t = new Thread(RunApi);
            t.Start();
        }

        private void RunApi()
        {
            while (runApi)
            {
                if (!UpdateAPIData())
                    return;
            }
        }

        private bool UpdateAPIData()
        {
            Single[] DriversTrackPct;
            Int32[] DriversLapNum;
            Int32[] DriversTrackSurface;

            if (Sdk.IsConnected())
            {
                if (Sdk.GetData("SessionNum") == null)
                    return true;

                int newUpdate = Sdk.Header.SessionInfoUpdate;
                if (newUpdate != lastUpdate)
                {
                    DataManager.Mutex.WaitOne();
                    lastUpdate = newUpdate;


                    float trackLength = DataManager.Track.Length;
                    yamlParser(Sdk.GetSessionInfo());

                    if (trackLength != DataManager.Track.Length)
                        DataManager.TimeDelta = new TimeDelta(DataManager.Track.Length, 10, 64);
                    DataManager.Mutex.ReleaseMutex();
                }

                if (readCache)
                {
                    DataManager.ReadCache(DataManager.Sessions.SessionId);
                    readCache = false;
                }

                // when session changes, reset prevtime
                if (currentime >= prevtime)
                    currentime = (Double)Sdk.GetData("SessionTime");
                else
                    prevtime = 0.0;

                // wait and lock
                DataManager.Mutex.WaitOne();

                DataManager.Sessions.SetCurrentSession((int)Sdk.GetData("SessionNum"));
                DataManager.Sessions.CurrentSession.SetFollowedDriver((int)Sdk.GetData("CamCarIdx"));
                DataManager.Sessions.CurrentSession.FollowedDriver.AddAirTime(currentime - prevtime);
                if (currentime > prevtime)
                {
                    DataManager.Sessions.CurrentSession.SessionTime = (double)Sdk.GetData("SessionTime");

                    // hide ui if needed
                    if (DataManager.ShowSimUi == false)
                    {
                        int currentCamState = (int)Sdk.GetData("CamCameraState");
                        if ((currentCamState & 0x0008) == 0)
                        {
                            Sdk.BroadcastMessage(iRSDKSharp.BroadcastMessageTypes.CamSetState, (currentCamState | 0x0008), 0);
                        }
                    }

                    if (DataManager.Sessions.CurrentSession.SessionStartTime < 0)
                    {
                        DataManager.Sessions.CurrentSession.SessionStartTime = DataManager.Sessions.CurrentSession.SessionTime;
                        DataManager.Sessions.CurrentSession.CurrentReplayPosition = (Int32)Sdk.GetData("ReplayFrameNum");
                    }

                    // clear delta between sessions
                    if (DataManager.Sessions.CurrentSession.SessionTime < 0.5)
                        DataManager.TimeDelta = new TimeDelta(DataManager.Track.Length, 10, 64);

                    DataManager.Sessions.CurrentSession.TimeRemaining = (Double)Sdk.GetData("SessionTimeRemain");

                    if (((Double)Sdk.GetData("SessionTime") - (Double)Sdk.GetData("ReplaySessionTime")) < 2)
                        timeoffset = (Int32)Sdk.GetData("ReplayFrameNum") - ((Double)Sdk.GetData("SessionTime") * 60);

                    SessionState prevState = DataManager.Sessions.CurrentSession.State;
                    DataManager.Sessions.CurrentSession.State = (SessionState)Sdk.GetData("SessionState");

                    if (prevState != DataManager.Sessions.CurrentSession.State)
                    {
                        SessionEvent ev = new SessionEvent(SessionEventType.State, DataManager.Sessions.CurrentSession.FollowedDriver.Driver,
                            "Session state changed to " + DataManager.Sessions.CurrentSession.State.ToString(), DataManager.Sessions.CurrentSession.SessionType,
                            DataManager.Sessions.CurrentSession.LapsComplete);
                        DataManager.Events.Add(ev);

                        // if state changes from racing to checkered update final lap
                        if (DataManager.Sessions.CurrentSession.SessionType == SessionType.Race &&
                            DataManager.Sessions.CurrentSession.FinishLine == Int32.MaxValue &&
                            prevState == SessionState.Racing &&
                            DataManager.Sessions.CurrentSession.State == SessionState.Checkered)
                        {
                            DataManager.Sessions.CurrentSession.FinishLine = (Int32)Math.Ceiling(DataManager.Sessions.CurrentSession.GetLeader().CurrentTrackPct);
                        }

                        // if new state is racing then trigger green flag
                        if (DataManager.Sessions.CurrentSession.State == SessionState.Racing && DataManager.Sessions.CurrentSession.Flag != SessionFlags.Invalid)
                            DataManager.Triggers.Push(TriggerType.FlagGreen);
                        else if (DataManager.Sessions.CurrentSession.State == SessionState.Checkered ||
                            DataManager.Sessions.CurrentSession.State == SessionState.Cooldown)
                            DataManager.Triggers.Push(TriggerType.FlagCheckered);
                        // before race show yellows
                        else if (DataManager.Sessions.CurrentSession.State == SessionState.Gridding ||
                            DataManager.Sessions.CurrentSession.State == SessionState.Pacing ||
                            DataManager.Sessions.CurrentSession.State == SessionState.Warmup)
                            DataManager.Triggers.Push(TriggerType.FlagYellow);

                        if (DataManager.Sessions.CurrentSession.State == SessionState.Racing &&
                            (prevState == SessionState.Pacing || prevState == SessionState.Gridding))
                            timeoffset = (Int32)Sdk.GetData("ReplayFrameNum") - ((Double)Sdk.GetData("SessionTime") * 60);
                    }

                    SessionFlags prevFlag = DataManager.Sessions.CurrentSession.Flag;
                    SessionStartLights prevLight = DataManager.Sessions.CurrentSession.StartLights;

                    parseFlag(DataManager.Sessions.CurrentSession, (Int32)Sdk.GetData("SessionFlags"));

                    // white flag handling
                    if (DataManager.Sessions.CurrentSession.LapsRemaining == 1 || DataManager.Sessions.CurrentSession.TimeRemaining <= 0)
                        DataManager.Sessions.CurrentSession.Flag = SessionFlags.White;

                    if (prevFlag != DataManager.Sessions.CurrentSession.Flag)
                    {
                        SessionEvent ev = new SessionEvent(
                            SessionEventType.Flag, DataManager.Sessions.CurrentSession.FollowedDriver.Driver, DataManager.Sessions.CurrentSession.Flag.ToString() + " flag",
                            DataManager.Sessions.CurrentSession.SessionType, DataManager.Sessions.CurrentSession.LapsComplete);
                        DataManager.Events.Add(ev);

                        if (DataManager.Sessions.CurrentSession.State == SessionState.Racing)
                        {
                            switch (DataManager.Sessions.CurrentSession.Flag)
                            {
                                case SessionFlags.Caution:
                                case SessionFlags.Yellow:
                                case SessionFlags.YellowWaving:
                                    DataManager.Triggers.Push(TriggerType.FlagYellow);
                                    break;
                                case SessionFlags.Checkered:
                                    DataManager.Triggers.Push(TriggerType.FlagCheckered);
                                    break;
                                case SessionFlags.White:
                                    if (DataManager.Sessions.CurrentSession.SessionType == SessionType.Race) // White flag only in Races!
                                        DataManager.Triggers.Push(TriggerType.FlagWhite);
                                    break;
                                default:
                                    DataManager.Triggers.Push(TriggerType.FlagGreen);
                                    break;
                            }
                        }

                        // yellow manual calc
                        if (DataManager.Sessions.CurrentSession.Flag == SessionFlags.Yellow)
                            DataManager.Sessions.CurrentSession.Cautions++;
                    }

                    if (prevLight != DataManager.Sessions.CurrentSession.StartLights)
                    {

                        switch (DataManager.Sessions.CurrentSession.StartLights)
                        {
                            case SessionStartLights.Off:
                                DataManager.Triggers.Push(TriggerType.LightsOff);
                                break;
                            case SessionStartLights.Ready:
                                DataManager.Triggers.Push(TriggerType.LightsReady);
                                break;
                            case SessionStartLights.Set:
                                DataManager.Triggers.Push(TriggerType.LightsSet);
                                break;
                            case SessionStartLights.Go:
                                DataManager.Triggers.Push(TriggerType.LightsGo);
                                break;
                        }

                        SessionEvent ev = new SessionEvent(SessionEventType.StartLights,
                            DataManager.Sessions.CurrentSession.FollowedDriver.Driver, "Start lights changed to " + DataManager.Sessions.CurrentSession.StartLights.ToString(),
                            DataManager.Sessions.CurrentSession.SessionType, DataManager.Sessions.CurrentSession.LapsComplete);

                        DataManager.Events.Add(ev);
                    }

                    DataManager.Camera.CurrentGroup = (Int32)Sdk.GetData("CamGroupNumber");

                    if ((DataManager.CurrentFollowedDriver != DataManager.Sessions.CurrentSession.FollowedDriver.Driver.NumberPlatePadded)
                        || (DataManager.Camera.CurrentGroup != DataManager.CurrentCam))
                    {

                        DataManager.CurrentFollowedDriver = DataManager.Sessions.CurrentSession.FollowedDriver.Driver.NumberPlatePadded;
                        DataManager.CurrentCam = DataManager.Camera.CurrentGroup;
                    }

                    DriversTrackPct = (Single[])Sdk.GetData("CarIdxLapDistPct");
                    DriversLapNum = (Int32[])Sdk.GetData("CarIdxLap");
                    DriversTrackSurface = (Int32[])Sdk.GetData("CarIdxTrackSurface");

                    // The Voice-Chat Stuff
                    int newRadioTransmitCaridx = (Int32)Sdk.GetData("RadioTransmitCarIdx");
                    if (newRadioTransmitCaridx != DataManager.CurrentRadioCarIdx)
                    {
                        if (newRadioTransmitCaridx == -1)
                            DataManager.Triggers.Push(TriggerType.RadioOff);
                        else
                            DataManager.Triggers.Push(TriggerType.RadioOn);
                        DataManager.CurrentRadioCarIdx = newRadioTransmitCaridx;
                    }

                    if (((Double)Sdk.GetData("SessionTime") - (Double)Sdk.GetData("ReplaySessionTime")) > 1.1)
                        DataManager.InReplay = true;
                    else
                        DataManager.InReplay = false;

                    for (Int32 i = 0; i <= Math.Min(64, DataManager.Drivers.Count); i++)
                    {
                        ResultItem driver = DataManager.Sessions.CurrentSession.FindDriver(i);
                        Double prevpos = driver.PrevTrackPct;
                        Double prevupdate = driver.PrevTrackPctUpdate;
                        Double curpos = DriversTrackPct[i];

                        // update current lap replay pos
                        driver.CurrentLap.ReplayPos = (Int32)(((Double)Sdk.GetData("SessionTime") * 60) + timeoffset);

                        if (currentime > prevupdate && curpos != prevpos)
                        {
                            Single speed = 0;

                            // calculate speed
                            if (curpos < 0.1 && prevpos > 0.9) // crossing s/f line
                                speed = (Single)((((curpos - prevpos) + 1) * (Double)DataManager.Track.Length) / (currentime - prevupdate));
                            else
                                speed = (Single)(((curpos - prevpos) * (Double)DataManager.Track.Length) / (currentime - prevupdate));

                            if (Math.Abs(driver.PrevSpeed - speed) < 1 && (curpos - prevpos) >= 0) // filter junk
                                driver.Speed = speed;

                            driver.PrevSpeed = speed;
                            driver.PrevTrackPct = curpos;
                            driver.PrevTrackPctUpdate = currentime;

                            // update track position
                            if (driver.Finished == false && (SurfaceType)DriversTrackSurface[i] != SurfaceType.NotInWorld)
                                driver.CurrentTrackPct = DriversLapNum[i] + DriversTrackPct[i] - 1;

                            // add new lap
                            if (curpos < 0.1 && prevpos > 0.9 && driver.Finished == false) // crossing s/f line
                            {
                                if ((SurfaceType)DriversTrackSurface[i] != SurfaceType.NotInWorld && speed > 0)
                                {
                                    Double now = currentime - ((curpos / (1 + curpos - prevpos)) * (currentime - prevtime));

                                    Sector sector = new Sector();
                                    sector.Number = driver.Sector;
                                    sector.Speed = driver.Speed;
                                    sector.Time = (Single)(now - driver.SectorBegin);
                                    sector.Begin = driver.SectorBegin;

                                    driver.CurrentLap.Sectors.Add(sector);
                                    driver.CurrentLap.Time = (Single)(now - driver.Begin);
                                    driver.CurrentLap.ClassPosition = DataManager.Sessions.CurrentSession.GetClassPosition(driver.Driver);
                                    if (DataManager.Sessions.CurrentSession.SessionType == SessionType.Race)
                                        driver.CurrentLap.Gap = (Single)driver.GapLive;
                                    else
                                        driver.CurrentLap.Gap = driver.CurrentLap.Time - DataManager.Sessions.CurrentSession.FastestLap;
                                    driver.CurrentLap.GapLaps = 0;


                                    if (driver.CurrentLap.LapNumber > 0 &&
                                        driver.Laps.FindIndex(l => l.LapNumber.Equals(driver.CurrentLap.LapNumber)) == -1 &&
                                        (DataManager.Sessions.CurrentSession.State != SessionState.Gridding ||
                                        DataManager.Sessions.CurrentSession.State != SessionState.Cooldown))
                                    {
                                        driver.Laps.Add(driver.CurrentLap);
                                    }

                                    driver.CurrentLap = new Lap();
                                    driver.CurrentLap.LapNumber = DriversLapNum[i];
                                    driver.CurrentLap.Gap = driver.PreviousLap.Gap;
                                    driver.CurrentLap.GapLaps = driver.PreviousLap.GapLaps;
                                    driver.CurrentLap.ReplayPos = (Int32)(((Double)Sdk.GetData("SessionTime") * 60) + timeoffset);
                                    driver.CurrentLap.SessionTime = DataManager.Sessions.CurrentSession.SessionTime;
                                    driver.SectorBegin = now;
                                    driver.Sector = 0;
                                    driver.Begin = now;

                                    // caution lap calc
                                    if (DataManager.Sessions.CurrentSession.Flag == SessionFlags.Yellow && driver.Position == 1)
                                        DataManager.Sessions.CurrentSession.CautionLaps++;

                                    // class laps led
                                    if (DataManager.Sessions.CurrentSession.GetClassLeader(driver.Driver.Car.CarClassName).Driver.CarIndex == driver.Driver.CarIndex && driver.CurrentLap.LapNumber > 1)
                                        driver.ClassLapsLed++;

                                    driver.Stopwatch.Restart();
                                }
                            }

                            // add sector times
                            if (DataManager.SelectedSectors.Count > 0 && driver.Driver.CarIndex >= 0)
                            {
                                for (int j = 0; j < DataManager.SelectedSectors.Count; j++)
                                {
                                    if (curpos > DataManager.SelectedSectors[j] && j > driver.Sector)
                                    {
                                        Double now = currentime - ((curpos - DataManager.SelectedSectors[j]) * (curpos - prevpos));
                                        Sector sector = new Sector();
                                        sector.Number = driver.Sector;
                                        sector.Time = (Single)(now - driver.SectorBegin);
                                        sector.Speed = driver.Speed;
                                        sector.Begin = driver.SectorBegin;
                                        driver.CurrentLap.Sectors.Add(sector);
                                        driver.SectorBegin = now;
                                        driver.Sector = j;
                                    }
                                }
                            }

                            // cross finish line
                            if (driver.CurrentLap.LapNumber + driver.CurrentLap.GapLaps >= DataManager.Sessions.CurrentSession.FinishLine &&
                                (SurfaceType)DriversTrackSurface[i] != SurfaceType.NotInWorld &&
                                DataManager.Sessions.CurrentSession.SessionType == SessionType.Race &&
                                driver.Finished == false)
                            {
                                // finishing the race
                                DataManager.Sessions.CurrentSession.UpdatePosition();
                                driver.CurrentTrackPct = (Math.Floor(driver.CurrentTrackPct) + 0.0064) - (0.0001 * driver.Position);
                                driver.Finished = true;
                            }

                            // add events
                            if (driver.Driver.CarIndex >= 0)
                            {
                                // update tracksurface
                                driver.TrackSurface = (SurfaceType)DriversTrackSurface[i];
                                driver.NotifyPosition();
                            }
                        }
                    }

                    DataManager.TimeDelta.Update(currentime, DriversTrackPct);
                    DataManager.Sessions.CurrentSession.UpdatePosition();

                    prevtime = currentime;
                    DataManager.CurrentSessionTime = currentime;

                    DataManager.ApiConnected = true;
                }

                DataManager.Mutex.ReleaseMutex();

                System.Threading.Thread.Sleep(4);

                return true;
            }
            else if (Sdk.IsInitialized)
            {
                Sdk.Shutdown();
                lastUpdate = -1;
                return false;
            }
            else
                return false;
        }

        private void yamlParser(string yaml)
        {
            int start = 0;
            int end = 0;
            int length = 0;

            length = yaml.Length;
            start = yaml.IndexOf("WeekendInfo:\n", 0, length);
            end = yaml.IndexOf("\n\n", start, length - start);

            string WeekendInfo = yaml.Substring(start, end - start);
            DataManager.Track.Length = (Single)parseDoubleValue(WeekendInfo, "TrackLength", "km") * 1000;
            DataManager.Track.Id = parseIntValue(WeekendInfo, "TrackID");
            DataManager.Track.NumberOfTurns = parseIntValue(WeekendInfo, "TrackNumTurns");
            DataManager.Track.City = parseStringValue(WeekendInfo, "TrackCity");
            DataManager.Track.Country = parseStringValue(WeekendInfo, "TrackCountry");
            DataManager.Track.Altitude = (Single)parseDoubleValue(WeekendInfo, "TrackAltitude", "m");

            DataManager.Track.Weather.Skies = parseStringValue(WeekendInfo, "TrackSkies");
            DataManager.Track.Weather.TrackTemp = (Single)parseDoubleValue(WeekendInfo, "TrackSurfaceTemp", "C");
            DataManager.Track.Weather.AirTemp = (Single)parseDoubleValue(WeekendInfo, "TrackAirTemp", "C");
            DataManager.Track.Weather.AirPressure = (Single)parseDoubleValue(WeekendInfo, "TrackAirPressure", "Hg");
            DataManager.Track.Weather.WindSpeed = (Single)parseDoubleValue(WeekendInfo, "TrackWindVel", "m/s");
            DataManager.Track.Weather.WindDirection = (Single)parseDoubleValue(WeekendInfo, "TrackWindDir", "rad");
            DataManager.Track.Weather.Humidity = parseIntValue(WeekendInfo, "TrackRelativeHumidity", "%");
            DataManager.Track.Weather.FogLevel = parseIntValue(WeekendInfo, "TrackFogLevel", "%");

            if (parseIntValue(WeekendInfo, "Official") == 0 &&
                parseIntValue(WeekendInfo, "SeasonID") == 0 &&
                parseIntValue(WeekendInfo, "SeriesID") == 0)
                DataManager.Sessions.Hosted = true;
            else
                DataManager.Sessions.Hosted = false;

            DataManager.Sessions.SessionId = parseIntValue(WeekendInfo, "SessionID");
            DataManager.Sessions.SubSessionId = parseIntValue(WeekendInfo, "SubSessionID");

            length = yaml.Length;
            start = yaml.IndexOf("DriverInfo:\n", 0, length);
            end = yaml.IndexOf("\n\n", start, length - start);

            string DriverInfo = yaml.Substring(start, end - start);

            length = DriverInfo.Length;
            start = DriverInfo.IndexOf(" Drivers:\n", 0, length);
            end = length;

            string Drivers = DriverInfo.Substring(start, end - start - 1);
            string[] driverList = Drivers.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string driver in driverList)
            {
                int userId = parseIntValue(driver, "UserID");
                if (userId < Int32.MaxValue && userId > 0)
                {
                    int index = DataManager.Drivers.FindIndex(d => d.UserId.Equals(userId));
                    if (index < 0 && parseStringValue(driver, "CarPath") != "safety pcfr500s" && parseStringValue(driver, "AbbrevName") != "Pace Car")
                    {
                        Driver driverItem = new Driver();
                        Car car = new Car();
                        driverItem.Car = car;

                        driverItem.FullName = parseStringValue(driver, "UserName");

                        if (parseStringValue(driver, "AbbrevName") != null)
                        {
                            string[] splitName = parseStringValue(driver, "AbbrevName").Split(',');
                            if (splitName.Length > 1)
                                driverItem.ShortName = splitName[1] + " " + splitName[0];
                            else
                                driverItem.ShortName = parseStringValue(driver, "AbbrevName");
                        }
                        driverItem.Initials = parseStringValue(driver, "Initials");
                        driverItem.ClubName = parseStringValue(driver, "ClubName");
                        driverItem.Car.CarNumber = parseStringValue(driver, "CarNumber");
                        driverItem.Car.CarId = parseIntValue(driver, "CarID");
                        // TODO: driverItem.Car.CarClassName = (SharedData.theme != null ? SharedData.theme.getCarClass(driverItem.Car.CarId) : "unknown");
                        driverItem.Car.CarClassId = parseIntValue(driver, "CarClassID");
                        driverItem.UserId = parseIntValue(driver, "UserID");
                        driverItem.CarIndex = parseIntValue(driver, "CarIdx");
                        driverItem.IRating = parseIntValue(driver, "IRating");

                        int liclevel = parseIntValue(driver, "LicLevel");
                        int licsublevel = parseIntValue(driver, "LicSubLevel");

                        switch (liclevel)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                driverItem.Licence = LicenceLevel.R;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                driverItem.Licence = LicenceLevel.D;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                                driverItem.Licence = LicenceLevel.C;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                                driverItem.Licence = LicenceLevel.B;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                                driverItem.Licence = LicenceLevel.A;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                                driverItem.Licence = LicenceLevel.P;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                                driverItem.Licence = LicenceLevel.WC;
                                driverItem.SafetyRating = licsublevel;
                                break;
                            default:
                                driverItem.Licence = LicenceLevel.None;
                                driverItem.SafetyRating = -1;
                                break;
                        }

                        // TODO: fix buggs
                        if (driverItem.Car.CarNumber == null)
                            driverItem.Car.CarNumber = "000";
                        if (driverItem.Initials == null)
                            driverItem.Initials = "";

                        DataManager.Drivers.Add(driverItem);
                    }

                    length = yaml.Length;
                    start = yaml.IndexOf("SessionInfo:\n", 0, length);
                    end = yaml.IndexOf("\n\n", start, length - start);

                    string SessionInfo = yaml.Substring(start, end - start);

                    length = SessionInfo.Length;
                    start = SessionInfo.IndexOf(" Sessions:\n", 0, length);
                    end = length;

                    string Sessions = SessionInfo.Substring(start, end - start);
                    string[] sessionList = Sessions.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);

                    // Get Current running Session
                    int _CurrentSession = (int)Sdk.GetData("SessionNum");

                    foreach (string session in sessionList)
                    {
                        int sessionNum = parseIntValue(session, "SessionNum");
                        if (sessionNum < Int32.MaxValue)
                        {
                            int sessionIndex = DataManager.Sessions.SessionList.FindIndex(s => s.Id.Equals(sessionNum));
                            if (sessionIndex < 0) // add new session item
                            {
                                Session sessionItem = new Session();
                                sessionItem.SessionNumber = sessionNum;
                                sessionItem.SessionLaps = parseIntValue(session, "SessionLaps");
                                sessionItem.SessionTime = parseFloatValue(session, "SessionTime", "sec");
                                sessionItem.SessionType = sessionTypeMap[parseStringValue(session, "SessionType")];

                                if (sessionItem.SessionType == SessionType.Race)
                                    sessionItem.FinishLine = parseIntValue(session, "SessionLaps") + 1;
                                else
                                    sessionItem.FinishLine = Int32.MaxValue;

                                if (sessionItem.FinishLine < 0)
                                    sessionItem.FinishLine = Int32.MaxValue;

                                sessionItem.Cautions = parseIntValue(session, "ResultsNumCautionFlags");
                                sessionItem.CautionLaps = parseIntValue(session, "ResultsNumCautionLaps");
                                sessionItem.LeadChanges = parseIntValue(session, "ResultsNumLeadChanges");
                                sessionItem.LapsComplete = parseIntValue(session, "ResultsLapsComplete");

                                length = session.Length;
                                start = session.IndexOf("   ResultsFastestLap:\n", 0, length);
                                end = length;
                                string ResultsFastestLap = session.Substring(start, end - start);

                                sessionItem.FastestLap = parseFloatValue(ResultsFastestLap, "FastestTime");
                                index = DataManager.Drivers.FindIndex(d => d.CarIndex.Equals(parseIntValue(ResultsFastestLap, "CarIdx")));
                                if (index >= 0)
                                {
                                    sessionItem.FastestLapDriver = DataManager.Drivers[index];
                                    sessionItem.FastestLapNum = parseIntValue(ResultsFastestLap, "FastestLap");
                                }
                                DataManager.Sessions.SessionList.Add(sessionItem);
                                sessionIndex = DataManager.Sessions.SessionList.FindIndex(s => s.SessionNumber.Equals(sessionNum));
                            }
                            else // update only non fixed fields
                            {
                                DataManager.Sessions.SessionList[sessionIndex].LeadChanges = parseIntValue(session, "ResultsNumLeadChanges");
                                DataManager.Sessions.SessionList[sessionIndex].LapsComplete = parseIntValue(session, "ResultsLapsComplete");

                                length = session.Length;
                                start = session.IndexOf("   ResultsFastestLap:\n", 0, length) + "   ResultsFastestLap:\n".Length;
                                end = length;
                                string ResultsFastestLap = session.Substring(start, end - start);


                                DataManager.Sessions.SessionList[sessionIndex].FastestLap = parseFloatValue(ResultsFastestLap, "FastestTime");
                                index = DataManager.Drivers.FindIndex(d => d.CarIndex.Equals(parseIntValue(ResultsFastestLap, "CarIdx")));
                                if (index >= 0)
                                {
                                    DataManager.Sessions.SessionList[sessionIndex].FastestLapDriver = DataManager.Drivers[index];
                                    DataManager.Sessions.SessionList[sessionIndex].FastestLapNum = parseIntValue(ResultsFastestLap, "FastestLap");
                                }
                            }


                            length = session.Length;
                            start = session.IndexOf("   ResultsPositions:\n", 0, length);
                            end = session.IndexOf("   ResultsFastestLap:\n", start, length - start);

                            string Standings = session.Substring(start, end - start);
                            string[] standingList = Standings.Split(new string[] { "\n   - " }, StringSplitOptions.RemoveEmptyEntries);

                            Int32 position = 1;
                            List<Driver> standingsDrivers = DataManager.Drivers.ToList();

                            foreach (string standing in standingList)
                            {
                                int carIdx = parseIntValue(standing, "CarIdx");
                                if (carIdx < Int32.MaxValue)
                                {
                                    ResultItem resultItem = new ResultItem();
                                    resultItem = DataManager.Sessions.SessionList[sessionIndex].FindDriver(carIdx);

                                    standingsDrivers.Remove(standingsDrivers.Find(s => s.CarIndex.Equals(carIdx)));

                                    if (DataManager.Sessions.SessionList[sessionIndex].SessionType == DataManager.Sessions.CurrentSession.SessionType)
                                    {
                                        if ((resultItem.CurrentTrackPct % 1.0) > 0.1)
                                        {
                                            resultItem.PreviousLap.Position = parseIntValue(standing, "Position");
                                            resultItem.PreviousLap.Gap = parseFloatValue(standing, "Time");
                                            resultItem.PreviousLap.GapLaps = parseIntValue(standing, "Lap");
                                            resultItem.CurrentLap.Position = parseIntValue(standing, "Position");
                                        }
                                    }

                                    if (resultItem.Driver.CarIndex < 0)
                                    {
                                        // insert item
                                        int driverIndex = DataManager.Drivers.FindIndex(d => d.CarIndex.Equals(carIdx));
                                        resultItem.SetDriver(carIdx);
                                        resultItem.FastestLap = parseFloatValue(standing, "FastestTime");
                                        resultItem.LapsLed = parseIntValue(standing, "LapsLed");
                                        resultItem.CurrentTrackPct = parseFloatValue(standing, "LapsDriven");
                                        resultItem.Laps = new List<Lap>();

                                        Lap newLap = new Lap();
                                        newLap.LapNumber = parseIntValue(standing, "LapsComplete");
                                        newLap.Time = parseFloatValue(standing, "LastTime");
                                        newLap.Position = parseIntValue(standing, "Position");
                                        newLap.Gap = parseFloatValue(standing, "Time");
                                        newLap.GapLaps = parseIntValue(standing, "Lap");
                                        newLap.Sectors = new List<Sector>(3);
                                        resultItem.Laps.Add(newLap);

                                        resultItem.CurrentLap = new Lap();
                                        resultItem.CurrentLap.LapNumber = parseIntValue(standing, "LapsComplete") + 1;
                                        resultItem.CurrentLap.Position = parseIntValue(standing, "Position");
                                        resultItem.CurrentLap.Gap = parseFloatValue(standing, "Time");
                                        resultItem.CurrentLap.GapLaps = parseIntValue(standing, "Lap");

                                        lock (DataManager.SharedDataLock)
                                        {
                                            DataManager.Sessions.SessionList[sessionIndex].Standings.Add(resultItem);
                                            DataManager.Sessions.SessionList[sessionIndex].UpdatePosition();
                                        }
                                    }

                                    int lapnum = parseIntValue(standing, "LapsComplete");
                                    resultItem.FastestLap = parseFloatValue(standing, "FastestTime");
                                    resultItem.LapsLed = parseIntValue(standing, "LapsLed");

                                    if (DataManager.Sessions.SessionList[sessionIndex].SessionType == DataManager.Sessions.CurrentSession.SessionType)
                                    {
                                        resultItem.PreviousLap.Time = parseFloatValue(standing, "LastTime");
                                    }

                                    if (DataManager.Sessions.CurrentSession.State == SessionState.Cooldown)
                                    {
                                        resultItem.CurrentLap.Gap = parseFloatValue(standing, "Time");
                                        resultItem.CurrentLap.GapLaps = parseIntValue(standing, "Lap");
                                        resultItem.CurrentLap.Position = parseIntValue(standing, "Position");
                                        resultItem.CurrentLap.LapNumber = parseIntValue(standing, "LapsComplete");
                                    }

                                    resultItem.Position = parseIntValue(standing, "Position");
                                    resultItem.NotifySelf();
                                    resultItem.NotifyLaps();

                                    position++;
                                }
                            }

                            // Trigger Overlay Event, but only in current active session
                            if ((DataManager.Sessions.SessionList[sessionIndex].FastestLap != DataManager.Sessions.SessionList[sessionIndex].PreviousFastestLap)
                                && (_CurrentSession == DataManager.Sessions.SessionList[sessionIndex].Id)
                                )
                            {
                                if (DataManager.Sessions.SessionList[sessionIndex].FastestLap > 0)
                                {
                                    SessionEvent ev = new SessionEvent(SessionEventType.FastLap, DataManager.Sessions.SessionList[sessionIndex].FastestLapDriver,
                                                "New session fastest lap (" + Utils.floatTime2String(DataManager.Sessions.SessionList[sessionIndex].FastestLap, 3, false) + ")",
                                                DataManager.Sessions.SessionList[sessionIndex].SessionType,
                                               DataManager.Sessions.SessionList[sessionIndex].FastestLapNum
                                            );

                                    DataManager.Events.Add(ev);
                                    // Push Event to Overlay
                                    DataManager.Triggers.Push(TriggerType.FastestLap);
                                }
                            }

                            // update/add position for drivers not in results
                            foreach (Driver driverItem in standingsDrivers)
                            {
                                ResultItem standingItem = DataManager.Sessions.SessionList[sessionIndex].FindDriver(driverItem.CarIndex);
                                if (standingItem.Driver.CarIndex < 0)
                                {
                                    standingItem.SetDriver(driverItem.CarIndex);
                                    standingItem.Position = position;
                                    standingItem.Laps = new List<Lap>();
                                    lock (DataManager.SharedDataLock)
                                    {
                                        DataManager.Sessions.SessionList[sessionIndex].Standings.Add(standingItem);
                                    }
                                    position++;
                                }
                                else
                                {
                                    standingItem.Position = position;
                                    position++;
                                }
                            }
                        }
                    }
                }
            }

            // add qualify session if it doesn't exist when race starts and fill it with YAML QualifyResultsInfo
            Session qualifySession = DataManager.Sessions.FindSessionByType(SessionType.Qualifying);
            if (qualifySession.SessionType == SessionType.None)
            {
                qualifySession.SessionType = SessionType.Qualifying;

                length = yaml.Length;
                start = yaml.IndexOf("QualifyResultsInfo:\n", 0, length);

                // if found
                if (start >= 0)
                {
                    end = yaml.IndexOf("\n\n", start, length - start);

                    string QualifyResults = yaml.Substring(start, end - start);

                    length = QualifyResults.Length;
                    start = QualifyResults.IndexOf(" Results:\n", 0, length);
                    end = length;

                    string Results = QualifyResults.Substring(start, end - start - 1);
                    string[] resultList = Results.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);

                    qualifySession.FastestLap = float.MaxValue;

                    foreach (string result in resultList)
                    {
                        if (result != " Results:")
                        {
                            ResultItem qualStandingsItem = qualifySession.FindDriver(parseIntValue(result, "CarIdx"));

                            if (qualStandingsItem.Driver.CarIndex > 0) // check if driver is in quali session
                            {
                                qualStandingsItem.Position = parseIntValue(result, "Position") + 1;
                            }
                            else // add driver to quali session
                            {
                                qualStandingsItem.SetDriver(parseIntValue(result, "CarIdx"));
                                qualStandingsItem.Position = parseIntValue(result, "Position") + 1;
                                qualStandingsItem.FastestLap = parseFloatValue(result, "FastestTime");
                                lock (DataManager.SharedDataLock)
                                {
                                    qualifySession.Standings.Add(qualStandingsItem);
                                }
                                // update session fastest lap
                                if (qualStandingsItem.FastestLap < qualifySession.FastestLap && qualStandingsItem.FastestLap > 0)
                                    qualifySession.FastestLap = qualStandingsItem.FastestLap;
                            }
                        }
                    }

                    DataManager.Sessions.SessionList.Add(qualifySession); // add quali session
                }
            }

            // get qualify results if race session standings is empty
            foreach (Session session in DataManager.Sessions.SessionList)
            {
                if (session.SessionType == SessionType.Race && session.Standings.Count < 1)
                {
                    length = yaml.Length;
                    start = yaml.IndexOf("QualifyResultsInfo:\n", 0, length);

                    // if found
                    if (start >= 0)
                    {
                        end = yaml.IndexOf("\n\n", start, length - start);

                        string QualifyResults = yaml.Substring(start, end - start);

                        length = QualifyResults.Length;
                        start = QualifyResults.IndexOf(" Results:\n", 0, length);
                        end = length;

                        string Results = QualifyResults.Substring(start, end - start - 1);
                        string[] resultList = Results.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string result in resultList)
                        {
                            if (result != " Results:")
                            {
                                ResultItem standingItem = new ResultItem();
                                standingItem.SetDriver(parseIntValue(result, "CarIdx"));
                                standingItem.Position = parseIntValue(result, "Position") + 1;
                                lock (DataManager.SharedDataLock)
                                {
                                    session.Standings.Add(standingItem);
                                }
                            }
                        }
                    }
                }
            }

            /*
            length = yaml.Length;
            start = yaml.IndexOf("CameraInfo:\n", 0, length);
            end = yaml.IndexOf("\n\n", start, length - start);

            string CameraInfo = yaml.Substring(start, end - start);

            length = CameraInfo.Length;
            start = CameraInfo.IndexOf(" Groups:\n", 0, length);
            end = length;

            string Cameras = CameraInfo.Substring(start, end - start - 1);
            string[] cameraList = Cameras.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);
            bool haveNewCam = false;
            foreach (string camera in cameraList)
            {
                int cameraNum = parseIntValue(camera, "GroupNum");
                if (cameraNum < Int32.MaxValue)
                {
                    CameraGroup camgrp = SharedData.Camera.FindId(cameraNum);
                    if (camgrp.Id < 0)
                    {
                        CameraGroup cameraGroupItem = new CameraGroup();
                        cameraGroupItem.Id = cameraNum;
                        cameraGroupItem.Name = parseStringValue(camera, "GroupName");
                        lock (SharedData.SharedDataLock)
                        {
                            SharedData.Camera.Groups.Add(cameraGroupItem);
                            haveNewCam = true;
                        }
                    }
                }
            }
            if (SharedData.settings.CamerasButtonColumn && haveNewCam) // If we have a new cam and want Camera Buttons, then forece a refresh of the main window buttons
                SharedData.refreshButtons = true;
            */

            length = yaml.Length;
            start = yaml.IndexOf("SplitTimeInfo:\n", 0, length);
            end = yaml.IndexOf("\n\n", start, length - start);

            string SplitTimeInfo = yaml.Substring(start, end - start);

            length = SplitTimeInfo.Length;
            start = SplitTimeInfo.IndexOf(" Sectors:\n", 0, length);
            end = length;

            string Sectors = SplitTimeInfo.Substring(start, end - start - 1);
            string[] sectorList = Sectors.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);

            if (sectorList.Length != DataManager.Sectors.Count)
            {
                DataManager.Sectors.Clear();
                foreach (string sector in sectorList)
                {
                    int sectornum = parseIntValue(sector, "SectorNum");
                    if (sectornum < 100)
                    {
                        float sectorborder = parseFloatValue(sector, "SectorStartPct");
                        DataManager.Sectors.Add(sectorborder);
                    }
                }

                // automagic sector selection
                if (DataManager.SelectedSectors.Count == 0)
                {
                    DataManager.SelectedSectors.Clear();

                    // load sectors
                    CfgFile sectorsIni = new CfgFile(Directory.GetCurrentDirectory() + "\\sectors.ini");
                    string sectorValue = sectorsIni.getValue("Sectors", DataManager.Track.Id.ToString(), false, String.Empty, false);
                    string[] selectedSectors = sectorValue.Split(';');
                    Array.Sort(selectedSectors);

                    DataManager.SelectedSectors.Clear();
                    if (sectorValue.Length > 0)
                    {
                        foreach (string sector in selectedSectors)
                        {
                            float number;
                            if (float.TryParse(sector, out number))
                                DataManager.SelectedSectors.Add(number);
                        }
                    }
                    else
                    {
                        if (DataManager.Sectors.Count == 2)
                        {
                            foreach (float sector in DataManager.Sectors)
                                DataManager.SelectedSectors.Add(sector);
                        }
                        else
                        {
                            float prevsector = 0;
                            foreach (float sector in DataManager.Sectors)
                            {

                                if (sector == 0 && DataManager.SelectedSectors.Count == 0)
                                {
                                    DataManager.SelectedSectors.Add(sector);
                                }
                                else if (sector >= 0.333 && DataManager.SelectedSectors.Count == 1)
                                {
                                    if (sector - 0.333 < Math.Abs(prevsector - 0.333))
                                        DataManager.SelectedSectors.Add(sector);
                                    else
                                        DataManager.SelectedSectors.Add(prevsector);
                                }
                                else if (sector >= 0.666 && DataManager.SelectedSectors.Count == 2)
                                {
                                    if (sector - 0.666 < Math.Abs(prevsector - 0.666))
                                        DataManager.SelectedSectors.Add(sector);
                                    else
                                        DataManager.SelectedSectors.Add(prevsector);
                                }

                                prevsector = sector;
                            }
                        }
                    }
                }
            }
        }

        public void StopApi()
        {
            runApi = false;
        }

        private string parseStringValue(string yaml, string key, string suffix = "")
        {
            int length = yaml.Length;
            int start = yaml.IndexOf(key, 0, length);
            if (start >= 0)
            {
                int end = yaml.IndexOf("\n", start, length - start);
                if (end < 0)
                    end = yaml.Length;
                start += key.Length + 2; // skip key name and ": " -separator
                if (start < end)
                    return yaml.Substring(start, end - start - suffix.Length).Trim();
                else
                    return null;
            }
            else
                return null;
        }

        private int parseIntValue(string yaml, string key, string suffix = "")
        {
            string parsedString = parseStringValue(yaml, key, suffix);
            int value;
            bool result = Int32.TryParse(parsedString, out value);
            if (result)
                return value;
            else
                return Int32.MaxValue;
        }

        private float parseFloatValue(string yaml, string key, string suffix = "")
        {
            string parsedString = parseStringValue(yaml, key, suffix);
            double value;
            bool result = Double.TryParse(parsedString, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-US"), out value);
            if (result)
                return (float)value;
            else
                return float.MaxValue;
        }

        private Double parseDoubleValue(string yaml, string key, string suffix = "")
        {
            string parsedString = parseStringValue(yaml, key, suffix);
            double value;
            bool result = Double.TryParse(parsedString, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-US"), out value);
            if (result)
                return value;
            else
                return Double.MaxValue;
        }

        private void parseFlag(Session session, Int64 flag)
        {

            Int64 regularFlag = flag & 0x0000000f;
            Int64 specialFlag = (flag & 0x0000f000) >> (4 * 3);
            Int64 startlight = (flag & 0xf0000000) >> (4 * 7);

            if (regularFlag == 0x8 || specialFlag >= 0x4)
                session.Flag = SessionFlags.Yellow;
            else if (regularFlag == 0x2)
                session.Flag = SessionFlags.White;
            else if (regularFlag == 0x1)
                session.Flag = SessionFlags.Checkered;
            else
                session.Flag = SessionFlags.Green;

            session.StartLights = (SessionStartLights)startlight;
        }
    }
}
