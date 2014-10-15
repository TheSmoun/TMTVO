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
using TMTVO_Api.ThemeApi;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für ImprovementElement.xaml
	/// </summary>
	public partial class ImprovementElement : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }

        public IThemeWindow ParentWindow { get; private set; }

		public ImprovementElement()
		{
			this.InitializeComponent();
		}

        public void FadeIn(LiveStandingsItem driver, int delay)
        {

        }

        public void FadeOut()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {

        }
    }
}