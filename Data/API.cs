using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yaml;

namespace TMTVO.Data
{
    public sealed class API
    {
        public bool Run { get; set; }

        private readonly int ticksPerSecond;
        private readonly List<Module> modules;
        private readonly iRacingSDK sdk;
        private Thread thread;

        public API(int ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            this.thread = new Thread(StartThread);

            modules = new List<Module>();
            sdk = new iRacingSDK();

            sdk.Startup();
        }

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
                    try
                    {
                        Thread.Sleep(sleepTime);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
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
            ConfigurationSection rootNode = Yaml.Yaml.Parse(lines);
            foreach (Module m in modules)
            {
                m.Update(rootNode);
            }
        }

        public void Start()
        {
            try
            {
                thread.Start();
            }
            catch (ThreadStateException)
            {
                thread.Resume();
            }
        }

        private void StartThread(object obj)
        {
            run();
        }

        public void Stop()
        {
            thread.Suspend();
        }
    }
}
