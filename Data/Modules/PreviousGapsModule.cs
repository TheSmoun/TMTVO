using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMTVO.Api;
using TMTVO.Widget;
using TMTVO.Widget.F1;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class PreviousGapsModule : Module
    {
        private Dictionary<Tuple<int, int>, LinkedList<float>> previousGaps;

        public PreviousGapsModule(PreviousGaps prevGaps) : base("PreviousGaps")
        {
            previousGaps = new Dictionary<Tuple<int, int>, LinkedList<float>>();
        }

        public float[] GetLastGaps(int carIndex1, int carIndex2, int count)
        {
            Tuple<int, int> t = new Tuple<int, int>(carIndex1, carIndex2);
            LinkedList<float> val = GetDictValue(t);
            if (val == null || val.Count < count)
                return null;

            float[] gaps = new float[count];
            LinkedListNode<float> node = val.Last;

            int i = count;
            int j = 0;
            while ((node = node.Previous) != null && i-- > 0)
                gaps[j++] = node.Value;

            return gaps;
        }

        public void AddGap(int carIndex1, int carIndex2, float gap)
        {
            Tuple<int, int> t = new Tuple<int, int>(carIndex1, carIndex2);
            LinkedList<float> val = GetDictValue(t);
            if (val == null)
            {
                val = new LinkedList<float>();
                previousGaps.Add(t, val);
            }

            val.AddLast(gap);
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            // TODO Implement
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            previousGaps.Clear();
        }

        private LinkedList<float> GetDictValue(Tuple<int, int> t)
        {
            LinkedList<float> val = null;
            if (previousGaps.TryGetValue(t, out val))
                return val;

            return null;
        }
    }
}
