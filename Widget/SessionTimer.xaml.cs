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

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für UserControl1.xaml
	/// </summary>
	public partial class SessionTimer : UserControl
	{
        private Dictionary<SessionType, string> SessionTypeToString = new Dictionary<SessionType, string>() {
            {SessionType.OpenPractice, "OP"},
            {SessionType.Qualify, "Q"},
            {SessionType.WarmUp, "W"},
            {SessionType.LapRace, "Lap"},
            {SessionType.TimeRace, "R"},
            {SessionType.TimeTrial, "T"}
        };

        private SessionTimerState state;
        public SessionMode Mode { get; set; }
        private bool visible;

        private int lastDrivenLaps = -1;
        private int lastTotalLaps = -1;

        private int lastSeconds = -1;

		public SessionTimer()
		{
			this.InitializeComponent();

            this.visible = false;
            this.Mode = SessionMode.LapMode;
            this.state = SessionTimerState.Normal;
		}

        public void FadeIn(SessionMode mode, SessionType type)
        {
            if (mode == SessionMode.LapMode)
                SwitchToLap();
            else
            {
                SwitchToTime();
                string s;
                SessionTypeToString.TryGetValue(type, out s);
                LapText2.Text = s;
            }

            if (visible)
                return;

            visible = true;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeInLap") as Storyboard;
            else
                sb = FindResource("FadeInTime") as Storyboard;
            sb.Begin();

            if (state == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeIn") as Storyboard;
                sb.Begin();
            }
        }

        public void FadeOut()
        {
            if (!visible)
                return;

            visible = false;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeOutLap") as Storyboard;
            else
                sb = FindResource("FadeOutTime") as Storyboard;
            sb.Begin();

            if (state == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeOut") as Storyboard;
                sb.Begin();
            }
        }

        public void YellowFlag()
        {
            if (state == SessionTimerState.Yellow || state == SessionTimerState.SafetyCar)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.TimeMode && state == SessionTimerState.Red)
                sb = FindResource("RedFadeOutTime") as Storyboard;
            else if (Mode == SessionMode.LapMode && state == SessionTimerState.Red)
                sb = FindResource("RedFadeOutLap") as Storyboard;

            if (sb != null)
                sb.Begin();

            if (Mode == SessionMode.LapMode)
                sb = FindResource("Yellow") as Storyboard;
            else
                sb = FindResource("YellowFadeInTime") as Storyboard;
            sb.Begin();

            state = SessionTimerState.Yellow;
        }

        public void RedFlag()
        {
            if (state == SessionTimerState.Red)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("Red") as Storyboard;
            else
                sb = FindResource("RedFadeInTime") as Storyboard;
            sb.Begin();

            sb = null;
            if (state == SessionTimerState.Yellow && Mode == SessionMode.LapMode)
                sb = FindResource("YellowFadeOutLap") as Storyboard;
            else if (state == SessionTimerState.SafetyCar)
                Normal();
            else if (state == SessionTimerState.Yellow && Mode == SessionMode.TimeMode)
                sb = FindResource("YellowFadeOutTime") as Storyboard;

            if (sb != null)
                sb.Begin();

            state = SessionTimerState.Red;
        }

        public void SafetyCarDeveloped()
        {
            if (state == SessionTimerState.SafetyCar)
                return;

            if (state != SessionTimerState.Yellow)
                YellowFlag();

            state = SessionTimerState.SafetyCar;
            Storyboard sb = FindResource("SafetyCarFadeIn") as Storyboard;
            sb.Begin();
        }

        public void SafetyCarIn()
        {
            if (state != SessionTimerState.SafetyCar)
                return;

            Normal();
        }

        public void Normal()
        {
            if (state == SessionTimerState.Normal)
                return;

            if (Mode == SessionMode.LapMode)
            {
                Storyboard sb = null;
                if (state == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                else if (state == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                }
                else if (state == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutLap") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }
            else
            {
                Storyboard sb = null;
                if (state == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                else if (state == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                }
                else if (state == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutTime") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }

            LapBackgroundCh.Opacity = 0;
            TimeBackgroundCh.Opacity = 0;

            state = SessionTimerState.Normal;
        }

        public void SwitchToTime()
        {
            if (Mode == SessionMode.TimeMode || visible)
                return;

            Mode = SessionMode.TimeMode;
        }

        public void SwitchToLap()
        {
            if (Mode == SessionMode.LapMode || visible)
                return;

            Mode = SessionMode.LapMode;
        }

        public void ChequeredFlag()
        {
            if (state == SessionTimerState.Chequered)
                return;

            if (state != SessionTimerState.Normal)
                Normal();

            if (Mode == SessionMode.LapMode)
                LapBackgroundCh.Opacity = 1;
            else
                TimeBackgroundCh.Opacity = 1;

            state = SessionTimerState.Chequered;
        }

        public void UpdateLaps(int drivenLaps, int maxLaps)
        {
            if (drivenLaps <= 0 || maxLaps <= 0 || drivenLaps > maxLaps || Mode != SessionMode.LapMode)
                return;

            if (lastDrivenLaps > 0 && lastDrivenLaps == lastTotalLaps)
                ChequeredFlag();
            else
                LapsText.Text = drivenLaps + " / " + maxLaps;
        }

        public void UpdateTime(int seconds)
        {
            if (seconds < 0 || Mode != SessionMode.TimeMode)
                return;

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

        private enum SessionTimerState
        {
            Yellow,
            SafetyCar,
            Normal,
            Red,
            Chequered
        }

        public enum SessionType
        {
            OpenPractice,
            Qualify,
            WarmUp,
            LapRace,
            TimeRace,
            TimeTrial
        }

        public enum SessionMode
        {
            LapMode,
            TimeMode
        }
	}
}