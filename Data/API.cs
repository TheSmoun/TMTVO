using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yaml;

namespace TMTVO.Data
{
    public sealed class API
    {
        public bool Run { get; set; }

        private readonly int ticksPerSecond;
        private readonly List<Module> modules;
        private readonly iRSDKSharp.iRacingSDK sdk;

        public API(int ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;

            modules = new List<Module>();
            sdk = new iRSDKSharp.iRacingSDK();

            sdk.Startup();
        }

        /// <summary>
        /// Should be called from another thread.
        /// </summary>
        public void run()
        {
            long maxDelay = 1000L / ticksPerSecond;

            while (Run)
            {
                long start = Environment.TickCount;

                UpdateModules();

                long end = Environment.TickCount;

                int sleepTime = (int) (maxDelay - (end - start));
                if (sleepTime < 0)
                {
                    Console.WriteLine("System overloaded!");
                }
                else
                {
                    System.Threading.Thread.Sleep(sleepTime);
                }
            }
        }

        public void AddModule(Module m)
        {
            modules.Add(m);
        }

        public void RemoveModule(Module m)
        {
            modules.Remove(m);
        }

        public void UpdateModules()
        {
            if (sdk.IsConnected())
            {
                UpdateModules(sdk.GetSessionInfo());              
            }
        }

        public void UpdateModules(string lines)
        {
            Node rootNode = Yaml.Yaml.Parse(lines);

            foreach (Module m in modules)
            {
                m.Update(rootNode);
            }
        }
    }
}
