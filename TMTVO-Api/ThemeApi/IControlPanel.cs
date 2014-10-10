using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO_Api.ThemeApi
{
    public interface IControlPanel
    {
        IThemeWindow ThemeWindow { get; }
    }
}
