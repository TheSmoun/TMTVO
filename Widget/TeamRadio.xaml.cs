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
	/// Interaktionslogik für TeamRadio.xaml
	/// </summary>
	public partial class TeamRadio : UserControl, IWidget
	{
        public bool Active { get; private set; }

		public TeamRadio()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
        }

        public void StartsSpeaking(string LastNameDriver, string driverNumber)
        {
            DriversNumber.Text = driverNumber;
            DriversName.Text = LastNameDriver;

            if (!Active)
                Active = true;

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }

        public void SetClubColor(Brush brush)
        {
            this.NumberPlate.Fill = brush;
        }
	}
}