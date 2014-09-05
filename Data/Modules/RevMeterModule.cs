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

        public RevMeterModule(RevMeter widget) : base("RevMeter")
        {
            this.widget = widget;
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            if (widget.Active)
                Application.Current.Dispatcher.Invoke(new Action(widget.Tick));
        }

        public override void Reset()
        {
            // TODO
        }
    }
}
