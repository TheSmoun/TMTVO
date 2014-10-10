using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für WECTVOverlay.xaml
    /// </summary>
    public partial class WECTVOverlay : Window
    {
        public WECTVOverlay()
        {
            InitializeComponent();
        }

        private void Overlay_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
