using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO;

namespace TMTVO_Api.ThemeApi
{
    public interface IThemeWindow
    {
        List<IWidget> Widgets { get; }
    }
}
