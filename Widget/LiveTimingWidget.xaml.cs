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
using TMTVO.Data.Modules;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für LiveStandingsWidget.xaml
	/// </summary>
	public partial class LiveTimingWidget : UserControl, IWidget
	{
        private static readonly double pageCd = 500D;

        public bool Active { get; private set; }
        public LinkedList<LiveTimingItem> Items;
        public LiveTimingItemMode Mode { get; set; }
        public LiveStandingsModule Module { get; set; }

        private Button prevPageButton;
        private Button nextPageButton;
        private bool canUpdateButtons;

        private int pageIndex;
        private Timer nextPageCd;
        private Timer prevPageCd;

		public LiveTimingWidget()
		{
			this.InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            canUpdateButtons = true;
            pageIndex = 0;
            nextPageCd = new Timer(pageCd);
            nextPageCd.Elapsed += LoadNextPage;

            prevPageCd = new Timer(pageCd);
            prevPageCd.Elapsed += LoadPrevPage;

            TvoControls tvoC = TMTVO.Controller.TMTVO.Instance.TvoControls;
            prevPageButton = tvoC.TimingPrevPage;
            nextPageButton = tvoC.TimingNextPage;

            Items = new LinkedList<LiveTimingItem>();
            int i = 1;

            LiveTimingItem item = Item1;
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

            foreach (UIElement elem in PageSwitcherInner.Children)
            {
                item = (LiveTimingItem)elem;
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
            pageIndex = 0;
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
            for (int i = 1; i <= Module.Items.Count; i++)
            {
                int pos = 1;
                if (i > 1)
                    pos = i + (pageIndex * 21);

                LiveStandingsItem item = Module.Items.Find(it => it.Position == pos);
                LiveTimingItem current = node.Value;
                current.Tick(item, Mode);

                node = node.Next;
                if (node == null)
                {
                    break;
                }
            }

            int j = (((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (pageIndex < j && canUpdateButtons)
                nextPageButton.IsEnabled = true;

            foreach (LiveTimingItem item in Items)
            {
                if (item.OldPosition == -1)
                    item.Visibility = Visibility.Hidden;
                else
                    item.Visibility = Visibility.Visible;
            }
        }

        public void NextPage()
        {
            canUpdateButtons = false;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                nextPageButton.IsEnabled = false;
                prevPageButton.IsEnabled = false;
            }));

            int i = (((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (pageIndex >= i)
                return;

            Storyboard sb = FindResource("NextPage") as Storyboard;
            sb.Begin();
            nextPageCd.Start();
        }

        private void LoadNextPage(object sender, ElapsedEventArgs e)
        {
            nextPageCd.Stop();
            LoadPage((((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0);
        }

        public void PrevPage()
        {
            canUpdateButtons = false;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                nextPageButton.IsEnabled = false;
                prevPageButton.IsEnabled = false;
            }));

            if (pageIndex <= 0)
                return;

            Storyboard sb = FindResource("PrevPage") as Storyboard;
            sb.Begin();
            prevPageCd.Start();
        }

        private void LoadPrevPage(object sender, ElapsedEventArgs e)
        {
            prevPageCd.Stop();
            LoadPage(pageIndex - 1);
        }

        private void LoadPage(int npi)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                FadeInPositions(npi);
            }));
        }

        private void FadeInPositions(int npi)
        {
            UIElementCollection uiE = PageSwitcherInner.Children;
            foreach (UIElement e in uiE)
                ((LiveTimingItem)e).FadeOut();

            pageIndex = npi;
            Storyboard sb = FindResource("NeutralizePage") as Storyboard;
            sb.Begin();

            for (int i = 0; i < uiE.Count; i++)
                ((LiveTimingItem)uiE[i]).FadeInLater(i * 25);

            canUpdateButtons = true;
            if (pageIndex > 0)
                prevPageButton.IsEnabled = true;
        }
    }
}