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
using TMTVO.Data.Modules;
using TMTVO.Widget.WEC;

namespace TMTVO
{
	/// <summary>
	/// Interaktionslogik für WECRaceTickerItem.xaml
	/// </summary>
	public partial class WECRaceTickerItem : UserControl
	{
		public WECRaceTickerItem()
		{
			this.InitializeComponent();
		}

        public bool Update(LiveStandingsItem item)
        {
            if (item == null)
                return false;

            Position.Text = item.PositionLive.ToString();
            TeamName.Text = item.Driver.Car.CarName;
            Gap.Text = item.GapTime.ToString("0.000"); // TODO get correct gap
            DriverName.Text = item.Driver.FullName;
            CarNumber.Text = item.Driver.Car.CarNumber;

            return true;
        }
    }
}