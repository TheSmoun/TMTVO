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
using TMTVO.Widget;
using TMTVO.Widget.F1;

namespace TMTVO
{
	/// <summary>
	/// Interaktionslogik für LapTimeElement.xaml
	/// </summary>
	public partial class LapTimeElement : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }

        private LiveStandingsItem driver;
        private LapTimeItemMode mode;

		public LapTimeElement()
		{
			this.InitializeComponent();
		}

        public void FadeIn(string title, LiveStandingsItem driver, int delay)
        {
            if (Active || driver == null)
                return;

            Active = true;
            this.driver = driver;
            if (title.StartsWith("BEST"))
                mode = LapTimeItemMode.Best;
            else if (title.StartsWith("LAST"))
                mode = LapTimeItemMode.Last;
            else
                return;

            TitleText.Text = title;
            Tick();
            (FindResource("FadeIn") as Storyboard).Begin();
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
            LapTime.Text = (mode == LapTimeItemMode.Best) ? driver.FastestLapTime.ConvertToTimeString() : driver.LastLapTime.ConvertToTimeString();
        }

        public void Reset()
        {
            Active = false;
            driver = null;
        }

        public enum LapTimeItemMode
        {
            Best,
            Last
        }
	}
}