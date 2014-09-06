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
            if (speed <= 180)
                SpeedGrid.Visibility = Visibility.Hidden;
            else
                SpeedGrid.Visibility = Visibility.Visible;

            if (speed <= 0)
            {
                SpeedGrid1Rotation.Angle = -57;
                SpeedGridRotation.Angle = 27;
            }
            else if (speed == 180)
            {
                SpeedGrid1Rotation.Angle = 94;
                SpeedGridRotation.Angle = 27;
            }
            else if (speed > 0 && speed < 180)
            {
                SpeedGridRotation.Angle = 27;
                SpeedGrid1Rotation.Angle = -57F + 151F * (speed / 180F);
            }
            else if (speed > 180 && speed < 360)
            {
                SpeedGrid1Rotation.Angle = 94;
                SpeedGridRotation.Angle = 27 + 153F * ((speed - 180) / 180F);
            }
            else if (speed >= 360)
            {
                SpeedGrid1Rotation.Angle = 94;
                SpeedGridRotation.Angle = 180;
            }

            Speed.Text = speed.ToString("0");
        }

        public void FadeIn(LiveStandingsItem driver)
        {
            if (Active || driver == null)
                return;

            Driver = driver;
            Active = true;
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }

        public void Tick()
        {
            float rpm = ((float[])Controller.TMTVO.Instance.Api.GetData("CarIdxRPM"))[Driver.Driver.CarIndex];
            if (rpm < 0)
            {
                FadeOut();
                return;
            }

            SetSpeed((int)(Driver.Speed * 3.6F));
            RPM.Text = rpm.ToString("0");
            Gear.Text = ((int[])Controller.TMTVO.Instance.Api.GetData("CarIdxGear"))[Driver.Driver.CarIndex].ToString("0");
        }
    }
}