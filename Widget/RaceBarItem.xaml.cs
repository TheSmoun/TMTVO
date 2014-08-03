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
	/// Interaktionslogik für RaceBarItem.xaml
	/// </summary>
	public partial class RaceBarItem : UserControl
	{
		public RaceBarItem()
		{
			this.InitializeComponent();
		}

        public void FadeIn()
        {
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }
    }
}