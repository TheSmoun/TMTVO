using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TMTVO.Data
{
    public sealed class DataManager
    {
        public static Mutex Mutex { get; set; }
        public static object SharedDataLock { get; set; }
        public static Track Track { get; set; }
        public static List<Driver> Drivers { get; set; }
        public static List<ResultItem> ResultItems { get; set; }
        public static Sessions Sessions { get; set; }
        public static List<SessionEvent> Events { get; set; }
        public static Stack Triggers { get; set; }
        public static List<float> Sectors { get; set; }
        public static List<float> SelectedSectors { get; set; }
        public static TimeDelta TimeDelta { get; set; }
        public static bool ShowSimUi { get; set; }
        public static bool InReplay { get; set; }
        public static double CurrentSessionTime { get; set; }
        public static int CurrentRadioCarIdx { get; set; }
        public static int CurrentTriggerCarIdx { get; set; }
        public static int CurrentFollowedDriver { get; set; }
        public static int CurrentCam { get; set; }
        public static int SelectedPlaySpeed { get; set; }
        public static CameraInfo Camera { get; set; }
        public static bool ApiConnected { get; set; }

        static DataManager()
        {
            Mutex = new Mutex();
            SharedDataLock = new object();
            Drivers = new List<Driver>();
            ResultItems = new List<ResultItem>();
            Track = new Track();
            Track.Weather = new Weather();
            Sessions = new Sessions();
            Events = new List<SessionEvent>();
            Triggers = new Stack();
            Sectors = new List<float>();
            SelectedSectors = new List<float>();
            Camera = new CameraInfo();
            CurrentRadioCarIdx = -1;
            CurrentTriggerCarIdx = -1;
            CurrentFollowedDriver = -1;
            CurrentCam = -1;
            SelectedPlaySpeed = 1;
            CurrentSessionTime = 0;
        }

        public static bool ReadCache(Int32 sessionId)
        {
            string cachefilename = Directory.GetCurrentDirectory() + "\\cache\\" + sessionId + "-sessions.xml";
            if (File.Exists(cachefilename))
            {
                FileStream fs = new FileStream(cachefilename, FileMode.Open);
                TextReader reader = new StreamReader(fs);
                XmlSerializer x = new XmlSerializer(DataManager.Sessions.GetType());
                DataManager.Sessions = (Sessions)x.Deserialize(reader);
                fs.Close();
            }
            else
                return false;

            cachefilename = Directory.GetCurrentDirectory() + "\\cache\\" + sessionId + "-drivers.xml";
            if (File.Exists(cachefilename))
            {
                FileStream fs = new FileStream(cachefilename, FileMode.Open);
                TextReader reader = new StreamReader(fs);
                XmlSerializer x = new XmlSerializer(DataManager.Drivers.GetType());
                DataManager.Drivers = (List<Driver>)x.Deserialize(reader);
                fs.Close();
            }
            else
                return false;

            return true;
        }

        public static void writeCache(Int32 sessionId)
        {
            DirectoryInfo di = Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\cache\\");
            TextWriter tw = new StreamWriter(Directory.GetCurrentDirectory() + "\\cache\\" + sessionId + "-sessions.xml");
            XmlSerializer x = new XmlSerializer(DataManager.Sessions.GetType());
            DataManager.Sessions = new Sessions();
            x.Serialize(tw, DataManager.Sessions);
            tw.Close();

            tw = new StreamWriter(Directory.GetCurrentDirectory() + "\\cache\\" + sessionId + "-drivers.xml");
            x = new XmlSerializer(DataManager.Drivers.GetType());
            DataManager.Drivers = new List<Driver>();
            x.Serialize(tw, DataManager.Drivers);
            tw.Close();
        }

        public static event PropertyChangedEventHandler PropertyChanged;

        public static void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(null, new PropertyChangedEventArgs(name));
        }
    }
}
