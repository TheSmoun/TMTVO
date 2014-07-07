using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yaml;

namespace TMTVO.Api
{
    public sealed class API
    {
        public iRacingSDK Sdk { get; private set; }
        public bool Run { get; set; }
        public bool IsConnected
        {
            get { return Sdk.IsConnected(); }
        }

        private readonly int ticksPerSecond;
        private readonly List<Module> modules;
        private Thread thread;

        public API(int ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            this.thread = new Thread(StartThread);

            modules = new List<Module>();
            Sdk = new iRacingSDK();

            Sdk.Startup();
        }

        private void RunApi()
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
            if (Sdk.IsConnected())
            {
                UpdateModules(Sdk.GetSessionInfo());
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
            RunApi();
        }

        public void Stop()
        {
            thread.Suspend();
        }

        public void HideUI()
        {

        }

        public void SwitchCamera(int driver, int camera)
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.CamSwitchNum, driver, camera);
        }

        public void ReplaySetPlaySpeed(int playspeed, int slowmotion)
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlaySpeed, playspeed, slowmotion);
        }

        public void ReplaySetPlayPosition(ReplayPositionModeTypes mode, int position)
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlayPosition, (int)mode, position);
        }

        public void ReplaySearch(ReplaySearchModeTypes mode, int position)
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySearch, (int)mode, 0);
        }

        public void Pause()
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0);
        }

        public void Play()
        {
            Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlaySpeed, 1, 0);
        }


        public object GetData(string key)
        {
            return Sdk.GetData(key);
        }
    }
}
