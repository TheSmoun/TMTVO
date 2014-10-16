using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TMTVO;
using TMTVO.Api;
using TMTVO_Api.ThemeApi;

namespace TMTVO_F1Theme
{
    public sealed class F1Theme : Theme
    {
        private F1TVOverlay window;
        private F1ControlTabs controls;

        private F1Theme(F1TVOverlay overlay, F1ControlTabs controls) : base(overlay, controls, ThemeType.F1)
        {
            this.window = overlay;
            this.controls = controls;
        }
        
        public static F1Theme Load()
        {
            F1TVOverlay overlay = new F1TVOverlay();
            F1ControlTabs controls = new F1ControlTabs(overlay, API.Instance);
            return new F1Theme(overlay, controls);
        }

        public override void FadeAllOut()
        {
            window.FadeAllOut();
        }

        public override void ShowOverlay()
        {
            window.Show();
            Active = true;
        }

        public override void Reset()
        {
            controls.Reset();
            window.Reset();
        }

        public override void Close()
        {
            window.Close();
            Active = false;
            controls.Visibility = Visibility.Hidden;
        }

        public override double Fps()
        {
            return 1 / (window.CurrentFps / 1000);
        }
    }
}
