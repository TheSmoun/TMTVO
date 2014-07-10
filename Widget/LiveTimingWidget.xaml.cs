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
	/// Interaktionslogik für LiveStandingsWidget.xaml
	/// </summary>
	public partial class LiveTimingWidget : UserControl, IWidget
	{
        public bool Active { get; private set; }

		public LiveTimingWidget()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            int i = 1;
            foreach (UIElement elem in LayoutRoot.Children)
            {
                LiveTimingItem item = (LiveTimingItem)elem;
                item.Flag.Visibility = Visibility.Hidden;
                item.InPit.Visibility = Visibility.Hidden;

                if (item.Name != "Item1")
                {
                    item.NumberLeader.Visibility = Visibility.Hidden;
                    item.BackgroundLeader.Visibility = Visibility.Hidden;
                    item.BackgroundLeader1.Visibility = Visibility.Hidden;
                }

                item.Position.Text = (i++).ToString();
            }
        }

        public void FadeIn()
        {
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

        public void Tick()
        {
            
        }
    }
}