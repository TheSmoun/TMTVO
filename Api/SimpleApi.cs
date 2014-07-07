using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMTVO.Data.Modules;

namespace TMTVO.Api
{
    public sealed class SimpleApi : IAPI
    {
        public iRacingSDK Sdk { get; private set; }
        public bool Run { get; set; }
        public bool IsConnected
        {
            get { return Sdk.IsConnected(); }
        }

        private readonly int ticksPerSecond;
        private readonly SessionTimerModule sessionTimerModule;
        private Thread thread;

        public SimpleApi(int ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            this.thread = new Thread(StartThread);

            Sdk = new iRacingSDK();

            Sdk.Startup();
        }

        public void AddModule(Module module) { }

        public void RemoveModule(Module module) { }

        public void UpdateModules()
        {
            if (Sdk.IsConnected())
            {
                UpdateModules(Sdk.GetSessionInfo());
            }
        }

        public void UpdateModules(string lines)
        {
            // TODO parse
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

        public void Stop()
        {
            thread.Suspend();
        }

        private void StartThread(object obj)
        {
            RunApi();
        }

        private void RunApi()
        {
            long maxDelay = 1000L / ticksPerSecond;

            while (Run)
            {
                long start = Environment.TickCount;

                UpdateModules();

                long end = Environment.TickCount;

                int sleepTime = (int)(maxDelay - (end - start));
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
