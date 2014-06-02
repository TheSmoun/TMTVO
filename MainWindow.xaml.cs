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

namespace TMTVO
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        private Controls controlWindow;
        private bool sessionTimerActive;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            controlWindow = new Controls(this);
            controlWindow.Show();
            Show();

            sessionTimerActive = false;
        }
    }
}
