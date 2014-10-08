using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using TMTVO.Data.Ini;
using Yaml;

namespace TMTVO.Api
{
    public sealed class API
    {
        public static API Instance { get; private set; }

        public double CurrentTime { get; private set; }
        public iRacingSDK Sdk { get; private set; }
        public IniFile Cars { get; private set; }

        public bool Run { get; set; }
        public long LastMS { get; private set; }
        public bool IsConnected
        {
            get { return Sdk.IsConnected(); }
        }

        private Mutex mutex;
        private readonly int ticksPerSecond;
        private readonly List<Module> modules;
        private Thread thread;
        private int nextConnectTry;
        private long time;

        public API(int ticksPerSecond)
        {
            mutex = new Mutex();
            this.ticksPerSecond = ticksPerSecond;

            Cars = new IniFile(Environment.CurrentDirectory + @"\cars.ini"); // TODO Pfad einstellen

            modules = new List<Module>();
            Sdk = new iRacingSDK();

            Instance = this;
        }

        private void RunApi()
        {
            long maxDelay = 1000L / ticksPerSecond;

            while (Run)
            {
                mutex.WaitOne();
                long start = Environment.TickCount;

                if (Sdk.IsConnected())
                {
                    CurrentTime = (double)Sdk.GetData("SessionTime");
                    UpdateModules();
                }
                else
                {
                    Sdk.Shutdown();
                    ResetModules();
                    Run = false;
                    return;
                }

                long end = Environment.TickCount;
                long time = end - start;
                this.time = time;
                int sleepTime = (int)(maxDelay - time);

                if (sleepTime <= 0)
                {
                    Console.WriteLine("System overloaded! " + time + "ms");
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
                mutex.ReleaseMutex();
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
                UpdateModules(Sdk.GetSessionInfo());
        }

        public void UpdateModules(string lines)
        {
            ConfigurationSection rootNode = Yaml.Yaml.Parse(lines);
            foreach (Module m in modules)
                m.Update(rootNode, this);
        }

        public void ResetModules()
        {
            foreach (Module m in modules)
                m.Reset();

            //Controller.TMTVO.Instance.Controls.Reset();
        }

        public void HideUI()
        {
            // TODO Implement
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

        public Module FindModule(string name)
        {
            return modules.Find(m => m.Name == name);
        }

        public void Connect(object sender, EventArgs e)
        {
            if (!IsConnected)
            {
                if (Environment.TickCount > nextConnectTry)
                {
                    Sdk.Startup();
                    nextConnectTry = Environment.TickCount + 1000;
                }
            }
            else
            {
                if (thread == null || !thread.IsAlive)
                {
                    thread = new Thread(new ThreadStart(RunApi));
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        public void UpdateControls(object sender, EventArgs e)
        {
            //Controller.TMTVO.Instance.Controls.FpsItem.Content = Controller.TMTVO.Instance.Window.CurrentFps + " FPS";

            if (time > 0)
                LastMS = time;

            //TMTVO.Controller.TMTVO.Instance.Controls.UpdateLaunchButton(this);
            //TMTVO.Controller.TMTVO.Instance.Controls.updateStatusBar(null, null);
        }
    }
}
