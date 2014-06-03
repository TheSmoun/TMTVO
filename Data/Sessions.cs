using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Sessions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public List<Session> SessionList { get; private set; }
        public bool Hosted { get; set; }
        public int SessionId { get; set; }
        public int SubSessionId { get; set; }

        private int currentSession;

        public Sessions()
        {
            SessionList = new List<Session>();
            Hosted = false;
            SessionId = 0;
            SubSessionId = 0;
            currentSession = 0;
        }

        public Session CurrentSession
        {
            get
            {
                if (SessionList.Count > 0)
                    return SessionList[currentSession];
                else
                    return new Session(); 
            }
        }

        public void SetCurrentSession(int id)
        {
            int index = SessionList.FindIndex(s => (s.SessionNumber == id));
            if (index >= 0)
            {
                if (currentSession != index)
                {
                    currentSession = index;
                    this.NotifyPropertyChanged("CurrentSession");
                }
            }
            else
            {
                currentSession = 0;
                this.NotifyPropertyChanged("CurrentSession");
            }
        }

        public Session FindSessionByType(SessionType type)
        {
            int index = SessionList.FindIndex(s => s.SessionType.Equals(type));
            if (index >= 0)
                return SessionList[index];
            else
                return new Session();
        }

        public int FindSessionIndexByType(SessionType type)
        {
            int index = SessionList.FindIndex(s => s.SessionType.Equals(type));
            if (index >= 0)
                return index;
            else
                return 0;
        }
    }
}
