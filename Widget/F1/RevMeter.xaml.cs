using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.Data.Modules;

namespace TMTVO.Widget.F1
{
	/// <summary>
	/// Interaktionslogik für RevMeter.xaml
	/// </summary>
	public partial class RevMeter : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public LiveStandingsItem Driver { get; private set; }

		public RevMeter()
		{
			this.InitializeComponent();
		}

        public void SetBrake(float brakePct)
        {
            if (brakePct <= 0)
                BrakeRotation.Angle = -51;
            else if (brakePct >= 1)
                BrakeRotation.Angle = 90;
            else
                BrakeRotation.Angle = 141 * brakePct - 51;
        }

        public void SetThrottle(float throttlePct)
        {
            if (throttlePct <= 0)
                ThrottleRotation.Angle = 50;
            else if (throttlePct >= 1)
                ThrottleRotation.Angle = -88;
            else
                ThrottleRotation.Angle = 138 * (1 - throttlePct) - 88;
        }

        public void SetSpeed(int speed)
        {
            if (speed <= 0)
                SpeedRotation.Angle = -59;
            else if (speed == 287)
                SpeedRotation.Angle = 180;
            else if (speed == 288)
                SpeedRotation.Angle = -180;
            else if (speed > 0 && speed < 287)
                SpeedRotation.Angle = -59F + (59F + 180F) * (speed / 287F);
            else if (speed >= 360)
                SpeedRotation.Angle = -117;
            else if (speed > 289 && speed < 360)
                SpeedRotation.Angle = -180 - (59F + 180F) * (speed / 287F);

            Speed.Text = speed.ToString("0");
        }

        public void FadeIn(LiveStandingsItem driver)
        {
            if (Active || driver == null)
                return;

            Driver = driver;
            Active = true;
            LayoutRoot.Visibility = Visibility.Visible; // TODO
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            LayoutRoot.Visibility = Visibility.Hidden; // TODO;
        }

        public void Tick()
        {
            SetSpeed((int)(Driver.Speed * 3.6F));
            RPM.Text = ((float[])Controller.TMTVO.Instance.Api.GetData("CarIdxRPM"))[Driver.Driver.CarIndex].ToString("0");
            // TODO Rest
        }
    }
}