using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TMTVO.Api;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class RevMeterModule : Module
    {
        private RevMeter widget;
        private SpeedCompareWidget sWidget;

        public RevMeterModule(RevMeter widget, SpeedCompareWidget sWidget) : base("RevMeter")
        {
            this.widget = widget;
            this.sWidget = sWidget;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            if (widget.Active)
                Application.Current.Dispatcher.BeginInvoke(new Action(widget.Tick));

            if (sWidget.Active)
                Application.Current.Dispatcher.BeginInvoke(new Action(sWidget.Tick));
        }

        public override void Reset()
        {
            // TODO
        }
    }
}
