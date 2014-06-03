using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class SessionEvent
    {
        public SessionEventType EventTpye { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Description { get; private set; }
        public Driver Driver { get; private set; }
        public int LapNumber { get; private set; }
        public int Rewind { get; set; }

        private SessionType session;

        public SessionEvent(SessionEventType type, Driver driver, string desc, SessionType session, int lap)
        {
            this.EventTpye = type;
            this.TimeStamp = DateTime.Now;
            this.Driver = driver;
            this.Description = desc;
            this.session = session;
            this.LapNumber = lap;
            this.Rewind = 0;
        }

        public string Session
        {
            get
            {
                return this.session.ToString();
            }
        }
    }
}
