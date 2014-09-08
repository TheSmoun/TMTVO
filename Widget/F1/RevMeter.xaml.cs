using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
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

namespace TMTVO.Widget.F1
{
	/// <summary>
	/// Interaktionslogik für RevMeter.xaml
	/// </summary>
	public partial class RevMeter : UserControl, IWidget
	{
        private static readonly Dictionary<int, string> gears = new Dictionary<int, string>()
        {
            {-1, "R"},
            {0, "N"},
            {1, "1"},
            {2, "2"},
            {3, "3"},
            {4, "4"},
            {5, "5"},
            {6, "6"},
            {7, "7"}
        };

        public bool Active { get; private set; }
        public LiveStandingsItem Driver { get; private set; }

        private Timer neutralCooldown;
        private int currentGear;
        private int prevGear;
        private bool canUpdateGear;

        private bool pushToPass;
        private bool prevPushToPass;

		public RevMeter()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            Driver = null;
            canUpdateGear = true;
            pushToPass = false;
            prevPushToPass = false;
            prevGear = -1;
            currentGear = 0;
            neutralCooldown = new Timer(250);
            neutralCooldown.Elapsed += neutralCooldown_Elapsed;
        }

        private void neutralCooldown_Elapsed(object sender, ElapsedEventArgs e)
        {
            neutralCooldown.Stop();
            canUpdateGear = true;
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

            if (speed <= 100)
            {
                Speed.Opacity = 0F;
                SpeedLow.Opacity = 1f;
            }
            else if (speed >= 200)
            {
                Speed.Opacity = 1F;
                SpeedLow.Opacity = 0f;
            }
            else
            {
                Speed.Opacity = 0F + ((speed - 100F) / 100F);
                SpeedLow.Opacity = 1f;
            }

            Speed.Text = speed.ToString("0");
            SpeedLow.Text = speed.ToString("0");
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

        private void updateGear()
        {
            if (prevGear > currentGear)
                Application.Current.Dispatcher.Invoke(new Action(shiftUp));
            else if (prevGear < currentGear)
                Application.Current.Dispatcher.Invoke(new Action(shiftDown));
            else
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    sb_Completed(null, null);
                }));

            currentGear = prevGear;
        }

        private void shiftUp()
        {
            if (prevGear >= 7)
                return;

            canUpdateGear = false;
            Storyboard sb = FindResource("GearPlus") as Storyboard;
            sb.Completed += sb_Completed;
            sb.Begin();
        }

        private void shiftDown()
        {
            if (prevGear <= -1)
                return;

            canUpdateGear = false;
            Storyboard sb = FindResource("GearMinus") as Storyboard;
            sb.Completed += sb_Completed;
            sb.Begin();
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = FindResource("ResetGears") as Storyboard;
                sb.Begin();

                Gear.Text = gears.GetGearValue(prevGear);
                Gear_1.Text = gears.GetGearValue(prevGear + 1);
                Gear_2.Text = gears.GetGearValue(prevGear - 2);
                Gear_3.Text = gears.GetGearValue(prevGear - 1);
                Gear_4.Text = gears.GetGearValue(prevGear + 2);
                Gear_4_Dummy.Text = gears.GetGearValue(prevGear + 3);
                Gear_2_Dummy.Text = gears.GetGearValue(prevGear - 3);
            }));

            canUpdateGear = true;
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

        private void fadeInP2P()
        {
            if (pushToPass)
                return;

            pushToPass = true;
            Storyboard sb = FindResource("P2pActivated") as Storyboard;
            sb.Begin();
        }

        private void fadeOutP2P()
        {
            if (!pushToPass)
                return;

            pushToPass = false;
            Storyboard sb = FindResource("P2pDisabled") as Storyboard;
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
            prevGear = ((int[])Controller.TMTVO.Instance.Api.GetData("CarIdxGear"))[Driver.Driver.CarIndex];

            prevPushToPass = false;                                                                                             // TODO get Push to pass value
            if (prevPushToPass && !pushToPass)
                Application.Current.Dispatcher.Invoke(new Action(fadeInP2P));
            else if (!prevPushToPass && pushToPass)
                Application.Current.Dispatcher.Invoke(new Action(fadeOutP2P));

            if (prevGear == 0 && canUpdateGear)
            {
                canUpdateGear = false;
                neutralCooldown.Start();
            }

            if (canUpdateGear)
                updateGear();
        }
    }
}