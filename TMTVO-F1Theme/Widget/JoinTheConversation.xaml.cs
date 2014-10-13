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
using TMTVO;
using TMTVO_Api.ThemeApi;

namespace TMTVO_F1Theme
{
	/// <summary>
	/// Interaktionslogik für JoinTheConversation.xaml
	/// </summary>
	public partial class JoinTheConversation : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public IThemeWindow ParentWindow { get; private set; }

        private Storyboard mainAnimation;

		public JoinTheConversation(IThemeWindow parent)
		{
			this.InitializeComponent();
            this.ParentWindow = parent;
            Active = false;
		}

        public void FadeIn(string title, string value)
        {
            if (Active || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(value))
                return;

            Active = true;
            TitleText.Text = title;
            TitleTextDummy.Text = title;
            ValueText.Text = value;
            ValueTextDummy.Text = value;

            Measure(new Size(350, 36));
            Arrange(new Rect(new Size(350, 36)));

            TitleText.Width = TitleTextDummy.ActualWidth;
            ValueText.Width = ValueTextDummy.ActualWidth;

            BackgroundBlack.Width = TitleTextDummy.ActualWidth + 20;
            Farbverlauf.Width = TitleTextDummy.ActualWidth + 20;
            FadeInColor.Width = BackgroundBlack.Width;
            Main.Width = BackgroundBlack.Width + 18;

            TitleText.Margin = new Thickness(18, 0.666, 0, 0);
            ValueText.Margin = new Thickness(18, 0.666, 332, 0);

            double rm1 = Main.Width - 18 - TitleTextDummy.ActualWidth;
            double rm2 = Main.Width - 23 - ValueTextDummy.ActualWidth;

            mainAnimation = new Storyboard();
            ThicknessAnimation titleAnimation = new ThicknessAnimation(new Thickness(18, 0.666, rm1, 0), new Thickness(18, 0.666, Main.Width - 18, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            titleAnimation.BeginTime = TimeSpan.FromSeconds(3);
            Storyboard.SetTarget(titleAnimation, TitleText);
            Storyboard.SetTargetProperty(titleAnimation, new PropertyPath(MarginProperty));

            ThicknessAnimation valueAnimation = new ThicknessAnimation(new Thickness(18, 0.666, 332, 0), new Thickness(18, 0.666, rm2, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            valueAnimation.BeginTime = TimeSpan.FromMilliseconds(3500);
            Storyboard.SetTarget(valueAnimation, ValueText);
            Storyboard.SetTargetProperty(valueAnimation, new PropertyPath(MarginProperty));

            DoubleAnimation widthAnimation1 = new DoubleAnimation(TitleText.Width + 20, ValueText.Width + 20, new Duration(TimeSpan.FromSeconds(1)));
            widthAnimation1.BeginTime = TimeSpan.FromSeconds(3);
            Storyboard.SetTarget(widthAnimation1, BackgroundBlack);
            Storyboard.SetTargetProperty(widthAnimation1, new PropertyPath(WidthProperty));

            DoubleAnimation widthAnimation2 = new DoubleAnimation(TitleText.Width + 20, ValueText.Width + 20, new Duration(TimeSpan.FromSeconds(1)));
            widthAnimation2.BeginTime = TimeSpan.FromSeconds(3);
            Storyboard.SetTarget(widthAnimation2, Farbverlauf);
            Storyboard.SetTargetProperty(widthAnimation2, new PropertyPath(WidthProperty));

            mainAnimation.Children.Add(titleAnimation);
            mainAnimation.Children.Add(valueAnimation);
            mainAnimation.Children.Add(widthAnimation1);
            mainAnimation.Children.Add(widthAnimation2);

            mainAnimation.Completed += mainAnimation_Completed;

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Completed += sb1_Completed;
            sb.Begin();
        }

        private void sb1_Completed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(mainAnimation.Begin));
        }

        private void mainAnimation_Completed(object sender, EventArgs e)
        {
            Timer t = new Timer(3000);
            t.AutoReset = false;
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(FadeOut));
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

        public void Tick()
        {
            // Do nothing.
        }
    }
}