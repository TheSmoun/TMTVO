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
using System.Timers;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für RaceBar.xaml
	/// </summary>
	public partial class RaceBar : UserControl, IWidget
	{
        public bool Active { get; private set; }

		public RaceBar()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
        }

        public void FadeOut()
        {
            // TODO
        }
	}
}