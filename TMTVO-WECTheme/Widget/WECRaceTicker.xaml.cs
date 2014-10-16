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
using TMTVO_Api.ThemeApi;

namespace TMTVO.Widget.WEC
{
	/// <summary>
	/// Interaktionslogik für WECRaceTicker.xaml
	/// </summary>
	public partial class WECRaceTicker : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public IThemeWindow ParentWindow { get; private set; }
        public LiveStandingsModule Module { get; set; }

        Storyboard tickerStoryboard;
        ThicknessAnimation tickerAnimation;

		public WECRaceTicker(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Active = false;
            tickerStoryboard = new Storyboard();
            tickerAnimation = new ThicknessAnimation();
            tickerAnimation.From = new Thickness(1700, 0, 0, 0);
            tickerStoryboard.Children.Add(tickerAnimation);
        }

        public void FadeIn()
        {
            if (Active)
                return;

            Active = true;
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
            tickerStoryboard.Begin();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
            tickerStoryboard.Stop();
        }

        public void Tick()
        {
            ItemHostInner.Children.Clear();
            
            int margin = 0;
            for (int i = 1; i <= Module.Items.Count; i++)
            {
                if (i > 1)
                {
                    WECRaceTickerSpacer spacer = new WECRaceTickerSpacer();
                    spacer.Margin = new Thickness(margin, 0, 0, 0);
                    margin += (int)spacer.ActualWidth;
                    ItemHostInner.Children.Add(spacer);
                }

                WECRaceTickerItem item = new WECRaceTickerItem();
                if (!item.Update(Module.Items.Find(it => it.PositionLive == i)))
                {
                    i--;
                    continue;
                }

                item.Margin = new Thickness(margin, 0, 0, 0);
                margin += (int)item.ActualWidth;
                ItemHostInner.Children.Add(item);

                tickerAnimation.To = new Thickness(ItemHostInner.ActualWidth, 0, 0, 0);
                tickerAnimation.RepeatBehavior = RepeatBehavior.Forever;
                tickerAnimation.Duration = TimeSpan.FromMilliseconds(ItemHostInner.Children.Count / 2D * 2750D);
            }
        }

        public void Reset()
        {
            
        }

        public enum WECTickerMode
        {
            Gap,
            Interval
        }
    }
}