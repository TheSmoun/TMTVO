using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Api
{
    public interface Component
    {
        void Update(Dictionary<string, object> dict, API api, Module caller);
    }
}
