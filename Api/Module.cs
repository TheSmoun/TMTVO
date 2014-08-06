using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaml;

namespace TMTVO.Api
{
    public abstract class Module
    {
        private readonly string name;
        private readonly List<Component> components;

        public Module(string name)
        {
            this.name = name;
            components = new List<Component>();
        }

        public void AddComponent(Component c)
        {
            if (c == null)
            {
                return;
            }

            components.Add(c);
        }

        public void RemoveComponent(Component c)
        {
            components.Remove(c);
        }

        public void ClearComponents()
        {
            components.Clear();
        }

        public void UpdateComponents()
        {
            foreach (Component c in components) {
                c.Update(null, null);
            }
        }

        public string Name
        {
            get { return name; }
        }

        public abstract void Update(ConfigurationSection rootNode, API api);
        public abstract void Reset();
    }
}
