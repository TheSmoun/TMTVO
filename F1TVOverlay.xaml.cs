using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using System.Windows.Threading;
using TMTVO.Data;
using TMTVO.Widget;

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class F1TVOverlay : Window
    {
        private Dictionary<string, Brush> RacingLicenceToBrush = new Dictionary<string, Brush>()
        {
            {"R", new SolidColorBrush(Colors.Red)},
            {"D", new SolidColorBrush(Colors.Orange)},
            {"C", new SolidColorBrush(Colors.Yellow)},
            {"B", new SolidColorBrush(Colors.Green)},
            {"A", new SolidColorBrush(Colors.Blue)},
            {"P", new SolidColorBrush(Colors.DarkGray)},
            {"W", new SolidColorBrush(Colors.LightGray)}
        };

        private Dictionary<string, Color> RacingLicenceToColor = new Dictionary<string, Color>()
        {
            {"R", Colors.Red},
            {"D", Colors.Orange},
            {"C", Colors.Yellow},
            {"B", Colors.Green},
            {"A", Colors.Blue},
            {"P", Colors.DarkGray},
            {"W", Colors.LightGray}
        };

        public F1TVOverlay()
        {
            InitializeComponent();
        }

        private void Overlay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
