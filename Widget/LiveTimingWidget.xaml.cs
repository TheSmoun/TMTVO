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

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für LiveStandingsWidget.xaml
	/// </summary>
	public partial class LiveTimingWidget : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public LinkedList<LiveTimingItem> Items;
        public LiveTimingItemMode Mode { get; set; }

        public LiveStandingsModule Module { get; set; }

		public LiveTimingWidget()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Items = new LinkedList<LiveTimingItem>();
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

                Items.AddLast(item);
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
            LinkedListNode<LiveTimingItem> node = Items.First;
            foreach (LiveStandingsItem item in Module.Items)
            {
                LiveTimingItem current = node.Value;
                current.Tick(item, Mode);

                node = node.Next;
                if (node == null)
                {
                    Items.AddLast(new LiveTimingItem());
                    node = Items.Last;
                }
            }

            foreach (LiveTimingItem item in Items)
            {
                if (item.OldPosition == -1)
                    item.Visibility = Visibility.Hidden;
                else if (item.Visibility == Visibility.Hidden || item.Visibility == Visibility.Collapsed)
                {
                    item.Visibility = Visibility.Visible;
                    item.PositionImproved();
                }
            }
        }
    }
}