﻿using System;
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
using TMTVO.Api;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO_Api.ThemeApi;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für TeamRadio.xaml
	/// </summary>
	public partial class TeamRadio : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public IThemeWindow ParentWindow { get; private set; }
        public TeamRadioModule Module { get; set; }

		public TeamRadio(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;
            Active = false;
		}

        public void StartsSpeaking(string LastNameDriver, string driverNumber, Color classColor)
        {
            DriversNumber.Text = driverNumber;
            DriversName.Text = LastNameDriver;
            NumberPlate.Fill = new SolidColorBrush(classColor);

            if (!Active)
                Active = true;

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
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

        public void SetClubColor(Brush brush)
        {
            this.NumberPlate.Fill = brush;
        }

        public void Tick()
        {
            if (Module == null)
                Module = API.Instance.FindModule("TeamRadio") as TeamRadioModule;

            if ((Module.SpeekingCarIndex == -1 || !Module.CanShowTeamRadio) && Active)
                FadeOut();
            else if (Module.SpeekingCarIndex != -1 && !Active && Module.CanShowTeamRadio)
            {
                Driver driver = ((DriverModule)API.Instance.FindModule("DriverModule")).Drivers.Find(d => d.CarIndex == Module.SpeekingCarIndex);
                if (driver != null)
                    ((F1TVOverlay)ParentWindow).TeamRadioFadeIn(driver);
            }
            else if (Module.SpeekingCarIndex != -1 && Module.CanShowTeamRadio)
            {
                Driver driver = ((DriverModule)API.Instance.FindModule("DriverModule")).Drivers.Find(d => d.CarIndex == Module.SpeekingCarIndex);
                if (driver != null)
                {
                    DriversNumber.Text = driver.Car.CarNumber;
                    DriversName.Text = driver.LastUpperName;
                    NumberPlate.Fill = new SolidColorBrush(driver.LicColor);
                }
                else
                    FadeOut();
            }
        }

        public void Reset()
        {
            
        }
    }
}