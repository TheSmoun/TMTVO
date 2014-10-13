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
using TMTVO.Api;
using TMTVO.Data.Modules;
using TMTVO_Api.ThemeApi;
namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für SpeedCompareWidget.xaml
	/// </summary>
	public partial class SpeedCompareWidget : UserControl, IWidget
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
        public IThemeWindow ParentWindow { get; private set; }

        private LiveStandingsItem driver1;
        private LiveStandingsItem driver2;

        private Timer neutralCooldown1;
        private Timer neutralCooldown2;
        private int currentGear1;
        private int currentGear2;
        private int prevGear1;
        private int prevGear2;
        private bool canUpdateGear1;
        private bool canUpdateGear2;
        private bool pushToPass1;
        private bool pushToPass2;
        private bool prevPushToPass1;
        private bool prevPushToPass2;

		public SpeedCompareWidget(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;

            canUpdateGear1 = true;
            canUpdateGear2 = true;
            pushToPass1 = false;
            pushToPass2 = false;
            prevPushToPass1 = false;
            prevPushToPass2 = false;
            prevGear1 = -1;
            prevGear2 = -1;
            currentGear1 = 0;
            currentGear2 = 0;
            neutralCooldown1 = new Timer(250);
            neutralCooldown1.Elapsed += neutralCooldown1_Elapsed;
            neutralCooldown2 = new Timer(250);
            neutralCooldown2.Elapsed += neutralCooldown2_Elapsed;
		}

        private void neutralCooldown1_Elapsed(object sender, ElapsedEventArgs e)
        {
            neutralCooldown1.Stop();
            canUpdateGear1 = true;
        }

        private void neutralCooldown2_Elapsed(object sender, ElapsedEventArgs e)
        {
            neutralCooldown2.Stop();
            canUpdateGear2 = true;
        }

        public void FadeIn(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (Active || driver1 == null || driver2 == null)
                return;

            Active = true;
            this.driver1 = driver1;
            this.driver2 = driver2;

            DriversNumber1.Text = driver1.Driver.Car.CarNumber;
            DriversNumber2.Text = driver2.Driver.Car.CarNumber;

            DriversName1.Text = driver1.Driver.LastUpperName;
            DriversName2.Text = driver2.Driver.LastUpperName;

            NumberPlate1.Fill = new SolidColorBrush(driver1.Driver.LicColor);
            NumberPlate2.Fill = new SolidColorBrush(driver2.Driver.LicColor);

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Completed += sb_Completed;
            sb.Begin();
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            if (Parent != null)
                ((Grid)this.Parent).Children.Remove(this);
        }

        private void setSpeeds(int speed1, int speed2)
        {
            if (speed1 <= 0)
                LeftSpeedRotation.Angle = -180;
            else if (speed1 > 360)
                LeftSpeedRotation.Angle = 0;
            else
                LeftSpeedRotation.Angle = -180 + 180F * (speed1 / 360F);

            if (speed1 <= 100)
            {
                Speed.Opacity = 0F;
                SpeedLow.Opacity = 1f;
            }
            else if (speed1 >= 200)
            {
                Speed.Opacity = 1F;
                SpeedLow.Opacity = 0f;
            }
            else
            {
                Speed.Opacity = 0F + ((speed1 - 100F) / 100F);
                SpeedLow.Opacity = 1f;
            }

            Speed.Text = speed1.ToString("0");
            SpeedLow.Text = speed1.ToString("0");

            if (speed2 <= 0)
                RightSpeedRotation.Angle = -180;
            else if (speed2 > 360)
                RightSpeedRotation.Angle = 0;
            else
                RightSpeedRotation.Angle = -180 + 180F * (speed2 / 360F);

            if (speed2 <= 100)
            {
                Speed_Copy.Opacity = 0F;
                SpeedLow_Copy.Opacity = 1f;
            }
            else if (speed2 >= 200)
            {
                Speed_Copy.Opacity = 1F;
                SpeedLow_Copy.Opacity = 0f;
            }
            else
            {
                Speed_Copy.Opacity = 0F + ((speed2 - 100F) / 100F);
                SpeedLow_Copy.Opacity = 1f;
            }

            Speed_Copy.Text = speed2.ToString("0");
            SpeedLow_Copy.Text = speed2.ToString("0");
        }

        private void setRevs(int rev1, int rev2)
        {
            if (rev1 <= 0)
                RevsFillLeft.Height = 0;
            else if (rev1 > 18000)
                RevsFillLeft.Height = 125;
            else
                RevsFillLeft.Height = 125F * (rev1 / 18000F);

            if (rev2 <= 0)
                RevsFillRight.Height = 0;
            else if (rev2 > 18000)
                RevsFillRight.Height = 125;
            else
                RevsFillRight.Height = 125F * (rev2 / 18000F);
        }

        private void updateGear1()
        {
            if (prevGear1 > currentGear1)
                Application.Current.Dispatcher.BeginInvoke(new Action(shiftUp1));
            else if (prevGear1 < currentGear1)
                Application.Current.Dispatcher.BeginInvoke(new Action(shiftDown1));
            else
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    sb1_Completed(null, null);
                }));

            currentGear1 = prevGear1;
        }

        private void sb1_Completed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Storyboard sb = FindResource("GearReset1") as Storyboard;
                sb.Begin();

                Gear.Text = gears.GetGearValue(prevGear1);
                Gear_1.Text = gears.GetGearValue(prevGear1 + 1);
                Gear_2.Text = gears.GetGearValue(prevGear1 - 2);
                Gear_3.Text = gears.GetGearValue(prevGear1 - 1);
                Gear_4.Text = gears.GetGearValue(prevGear1 + 2);
                Gear_4_Dummy.Text = gears.GetGearValue(prevGear1 + 3);
                Gear_2_Dummy.Text = gears.GetGearValue(prevGear1 - 3);
            }));

            canUpdateGear1 = true;
        }

        private void shiftUp1()
        {
            if (prevGear1 >= 7)
                return;

            canUpdateGear1 = false;
            Storyboard sb = FindResource("GearPlus1") as Storyboard;
            sb.Completed += sb1_Completed;
            sb.Begin();
        }

        private void shiftDown1()
        {
            if (prevGear1 <= -1)
                return;

            canUpdateGear1 = false;
            Storyboard sb = FindResource("GearMinus1") as Storyboard;
            sb.Completed += sb1_Completed;
            sb.Begin();
        }

        private void updateGear2()
        {
            if (prevGear2 > currentGear2)
                Application.Current.Dispatcher.BeginInvoke(new Action(shiftUp2));
            else if (prevGear2 < currentGear2)
                Application.Current.Dispatcher.BeginInvoke(new Action(shiftDown2));
            else
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    sb2_Completed(null, null);
                }));

            currentGear2 = prevGear2;
        }

        private void sb2_Completed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Storyboard sb = FindResource("GearReset2") as Storyboard;
                sb.Begin();

                Gear1.Text = gears.GetGearValue(prevGear2);
                Gear_5.Text = gears.GetGearValue(prevGear2 + 1);
                Gear_6.Text = gears.GetGearValue(prevGear2 - 2);
                Gear_6.Text = gears.GetGearValue(prevGear2 - 1);
                Gear_8.Text = gears.GetGearValue(prevGear2 + 2);
                Gear_4_Dummy1.Text = gears.GetGearValue(prevGear2 + 3);
                Gear_2_Dummy1.Text = gears.GetGearValue(prevGear2 - 3);
            }));

            canUpdateGear2 = true;
        }

        private void shiftUp2()
        {
            if (prevGear2 >= 7)
                return;

            canUpdateGear2 = false;
            Storyboard sb = FindResource("GearPlus2") as Storyboard;
            sb.Completed += sb2_Completed;
            sb.Begin();
        }

        private void shiftDown2()
        {
            if (prevGear2 <= -1)
                return;

            canUpdateGear2 = false;
            Storyboard sb = FindResource("GearMinus2") as Storyboard;
            sb.Completed += sb2_Completed;
            sb.Begin();
        }

        private void setPushToPass(bool p2p1, bool p2p2)
        {
            // TODO implement
        }

        public void Tick()
        {
            float[] rpms = (float[])API.Instance.GetData("CarIdxRPM");
            float rpm1 = rpms[driver1.Driver.CarIndex];
            float rpm2 = rpms[driver2.Driver.CarIndex];
            if (rpm1 < 0 || rpm2 < 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(FadeOut));
                return;
            }

            setSpeeds((int)(driver1.SpeedKmh), (int)(driver2.SpeedKmh));
            setRevs((int)rpm1, (int)rpm2);

            int[] gears = (int[])API.Instance.GetData("CarIdxGear");
            prevGear1 = gears[driver1.Driver.CarIndex];
            prevGear2 = gears[driver2.Driver.CarIndex];

            /*prevPushToPass1 = false;                                                                                             // TODO get Push to pass value
            prevPushToPass2 = false;                                                                                             // TODO get Push to pass value
            if (prevPushToPass1 && !pushToPass1)
                Application.Current.Dispatcher.Invoke(new Action(fadeInP2P));
            else if (!prevPushToPass && pushToPass)
                Application.Current.Dispatcher.Invoke(new Action(fadeOutP2P));*/

            if (prevGear1 == 0 && canUpdateGear1)
            {
                canUpdateGear1 = false;
                neutralCooldown1.Start();
            }

            if (prevGear2 == 0 && canUpdateGear2)
            {
                canUpdateGear2 = false;
                neutralCooldown2.Start();
            }

            if (canUpdateGear1)
                updateGear1();

            if (canUpdateGear2)
                updateGear2();
        }
    }
}