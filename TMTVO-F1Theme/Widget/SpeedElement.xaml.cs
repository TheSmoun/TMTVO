using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
using TMTVO.Widget;
using TMTVO_Api.ThemeApi;

namespace TMTVO_F1Theme
{
	/// <summary>
	/// Interaktionslogik für SpeedElement.xaml
	/// </summary>
	public partial class SpeedElement : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }
        public IThemeWindow ParentWindow { get; private set; }
        public LiveStandingsItem Driver { get; internal set; }
        public int TopSpeedPosition { get; internal set; }

        public SpeedElement(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;
		}

        public void FadeIn(int pos, LiveStandingsItem driver, int delay)
        {
            if (Active || driver == null)
                return;

            Active = true;

            this.Driver = driver;
            this.TopSpeedPosition = pos;
            if (pos == 1)
                NumberLeader.Visibility = Visibility.Visible;
            else
                NumberLeader.Visibility = Visibility.Hidden;

            Position.Text = pos.ToString();
            ClassColorLeader.Color = ClassColorNormal.Color = driver.Driver.LicColor;
            ThreeLetterCode.Text = driver.Driver.ThreeLetterCode;
            Speed.Text = driver.TopSpeedKmh.ToString("0.0").Replace(',', '.');

            Thread t = new Thread(fadeInLater);
            t.Start(delay);
        }

        private void fadeInLater(object obj)
        {
            Thread.Sleep((int)obj);
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                (FindResource("FadeIn") as Storyboard).Begin();
            }));
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            (FindResource("FadeOut") as Storyboard).Begin();
        }

        public void Tick()
        {
            if (TopSpeedPosition == 1)
                NumberLeader.Visibility = Visibility.Visible;
            else
                NumberLeader.Visibility = Visibility.Hidden;

            Position.Text = TopSpeedPosition.ToString();
            ClassColorLeader.Color = ClassColorNormal.Color = Driver.Driver.LicColor;
            ThreeLetterCode.Text = Driver.Driver.ThreeLetterCode;
            Speed.Text = Driver.TopSpeedKmh.ToString("0.0").Replace(',', '.');
        }

        public void Reset()
        {
            ParentWindow = null;
            Active = false;
        }
    }
}