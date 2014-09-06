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

        private void setSpeed(int speed)
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

        private void setRev(int rev)
        {
            if (rev <= 9000)
                RevGrid.Visibility = Visibility.Hidden;
            else
                RevGrid.Visibility = Visibility.Visible;

            if (rev <= 0)
            {
                RevGrid1Rotation.Angle = -47;
                RevGridRotation.Angle = 28;
            }
            else if (rev == 9000)
            {
                RevGrid1Rotation.Angle = 94;
                RevGridRotation.Angle = 28;
            }
            else if (rev > 0 && rev < 9000)
            {
                RevGridRotation.Angle = 27;
                RevGrid1Rotation.Angle = -47F + 141F * (rev / 9000F);
            }
            else if (rev > 9000 && rev < 18000)
            {
                RevGrid1Rotation.Angle = 94;
                RevGridRotation.Angle = 28 + 139F * ((rev - 9000) / 9000F);
            }
            else if (rev >= 18000)
            {
                RevGrid1Rotation.Angle = 94;
                RevGridRotation.Angle = 167;
            }

            RPM.Text = rev.ToString("0");
        }

        public void FadeIn(LiveStandingsItem driver)
        {
            if (Active || driver == null)
                return;

            float rpm = ((float[])Controller.TMTVO.Instance.Api.GetData("CarIdxRPM"))[driver.Driver.CarIndex];
            if (rpm < 0)
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
                Application.Current.Dispatcher.Invoke(new Action(FadeOut));
                return;
            }

            setSpeed((int)(Driver.Speed * 3.6F));
            setRev((int)rpm);

            int gear = ((int[])Controller.TMTVO.Instance.Api.GetData("CarIdxGear"))[Driver.Driver.CarIndex];
            if (gear == -1)
                Gear.Text = "R";
            else if (gear == 0)
                Gear.Text = "N";
            else if (gear > 0)
                Gear.Text = gear.ToString("0");
            else
                Application.Current.Dispatcher.Invoke(new Action(FadeOut));
        }
    }
}