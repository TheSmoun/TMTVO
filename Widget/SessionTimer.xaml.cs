using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.Data;
using TMTVO.Data.Modules;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für UserControl1.xaml
	/// </summary>
	public partial class SessionTimer : UserControl, IWidget
	{
        private Dictionary<SessionType, string> SessionTypeToString = new Dictionary<SessionType, string>() {
            {SessionType.OfflineTesting, "T"},
            {SessionType.Practice, "P"},
            {SessionType.Qualifying, "Q"},
            {SessionType.WarmUp, "W"},
            {SessionType.LapRace, "Lap"},
            {SessionType.TimeRace, "R"},
            {SessionType.TimeTrial, "T"}
        };

        public SessionTimerState State { get; private set; }
        public SessionMode Mode { get; set; }
        public bool Active { get; private set; }

        private int lastDrivenLaps = -1;
        private int lastTotalLaps = -1;
        private int lastSeconds = -1;

        public SessionTimerModule Module { get; set; }

		public SessionTimer()
		{
			this.InitializeComponent();

            this.Active = false;
            this.Mode = SessionMode.LapMode;
            this.State = SessionTimerState.Normal;
		}

        public void FadeIn(SessionMode mode)
        {
            if (mode == SessionMode.LapMode)
                SwitchToLap();
            else
                SwitchToTime();

            if (Active)
                return;

            Active = true;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeInLap") as Storyboard;
            else
                sb = FindResource("FadeInTime") as Storyboard;
            sb.Begin();

            if (State == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeIn") as Storyboard;
                sb.Begin();
            }
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeOutLap") as Storyboard;
            else
                sb = FindResource("FadeOutTime") as Storyboard;
            sb.Begin();

            if (State == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeOut") as Storyboard;
                sb.Begin();
            }
        }

        public void YellowFlag()
        {
            if (State == SessionTimerState.Yellow || State == SessionTimerState.SafetyCar)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.TimeMode && State == SessionTimerState.Red)
                sb = FindResource("RedFadeOutTime") as Storyboard;
            else if (Mode == SessionMode.LapMode && State == SessionTimerState.Red)
                sb = FindResource("RedFadeOutLap") as Storyboard;

            if (sb != null)
                sb.Begin();

            if (Mode == SessionMode.LapMode)
                sb = FindResource("Yellow") as Storyboard;
            else
                sb = FindResource("YellowFadeInTime") as Storyboard;
            sb.Begin();

            State = SessionTimerState.Yellow;
        }

        public void RedFlag()
        {
            if (State == SessionTimerState.Red)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("Red") as Storyboard;
            else
                sb = FindResource("RedFadeInTime") as Storyboard;
            sb.Begin();

            sb = null;
            if (State == SessionTimerState.Yellow && Mode == SessionMode.LapMode)
                sb = FindResource("YellowFadeOutLap") as Storyboard;
            else if (State == SessionTimerState.SafetyCar)
                Normal();
            else if (State == SessionTimerState.Yellow && Mode == SessionMode.TimeMode)
                sb = FindResource("YellowFadeOutTime") as Storyboard;

            if (sb != null)
                sb.Begin();

            State = SessionTimerState.Red;
        }

        public void SafetyCarDeveloped()
        {
            if (State == SessionTimerState.SafetyCar)
                return;

            if (State != SessionTimerState.Yellow)
                YellowFlag();

            State = SessionTimerState.SafetyCar;
            Storyboard sb = FindResource("SafetyCarFadeIn") as Storyboard;
            sb.Begin();
        }

        public void SafetyCarIn()
        {
            if (State != SessionTimerState.SafetyCar)
                return;

            Normal();
        }

        public void Normal()
        {
            if (State == SessionTimerState.Normal)
                return;

            if (Mode == SessionMode.LapMode)
            {
                Storyboard sb = null;
                if (State == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                else if (State == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                }
                else if (State == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutLap") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }
            else
            {
                Storyboard sb = null;
                if (State == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                else if (State == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                }
                else if (State == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutTime") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }

            LapBackgroundCh.Opacity = 0;
            TimeBackgroundCh.Opacity = 0;

            State = SessionTimerState.Normal;
        }

        public void SwitchToTime()
        {
            if (Mode == SessionMode.TimeMode || Active)
                return;

            Mode = SessionMode.TimeMode;
        }

        public void SwitchToLap()
        {
            if (Mode == SessionMode.LapMode || Active)
                return;

            Mode = SessionMode.LapMode;
        }

        public void ChequeredFlag()
        {
            if (State == SessionTimerState.Chequered)
                return;

            if (State != SessionTimerState.Normal)
                Normal();

            if (Mode == SessionMode.LapMode)
                LapBackgroundCh.Opacity = 1;
            else
                TimeBackgroundCh.Opacity = 1;

            State = SessionTimerState.Chequered;
        }

        public void UpdateLaps(int drivenLaps, int maxLaps)
        {
            if (drivenLaps < 0 || maxLaps <= 0 || drivenLaps > maxLaps || Mode != SessionMode.LapMode)
            {
                LapsText.Text = "-- / --";
                return;
            }

            if (lastDrivenLaps > 0 && lastDrivenLaps == lastTotalLaps)
                ChequeredFlag();
            else
                LapsText.Text = drivenLaps + " / " + maxLaps;
        }

        public void UpdateTime(int seconds)
        {
            if (seconds < 0 || Mode != SessionMode.TimeMode)
            {
                LapText2.Text = "-:--";
                return;
            }

            if (lastSeconds == 0)
                ChequeredFlag();
            else
            {
                int s = seconds % 60;
                int m = (seconds / 60) % 60;
                int h = seconds / 3600;

                StringBuilder sb = new StringBuilder();
                if (h != 0)
                    sb.Append(h).Append(":");

                if (h != 0 && m < 10)
                    sb.Append("0");

                sb.Append(m).Append(":");

                if (s < 10)
                    sb.Append("0");

                sb.Append(s);

                LapsText2.Text = sb.ToString();
            }
        }

        public enum SessionTimerState
        {
            Yellow,
            SafetyCar,
            Normal,
            Red,
            Chequered
        }

        public enum SessionMode
        {
            LapMode,
            TimeMode
        }

        public void Tick()
        {
            string sType = "";
            if (SessionTypeToString.TryGetValue(Module.SessionType, out sType))
                LapText2.Text = sType;

            this.UpdateTime(Module.TimeRemaining);
            this.UpdateLaps(Module.LapsDriven, Module.LapsTotal);

            switch (Module.SessionFlags)
            {
                case SessionFlags.Green:
                    Normal();
                    break;
                case SessionFlags.Yellow:
                    YellowFlag();
                    break;
                case SessionFlags.Checkered:
                    ChequeredFlag();
                    break;
                default:
                    break;
            }
        }
    }
}