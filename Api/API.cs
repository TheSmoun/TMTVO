﻿using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Yaml;

namespace TMTVO.Api
{
    public sealed class API
    {
        public double CurrentTime { get; private set; }
        public iRacingSDK Sdk { get; private set; }
        public bool Run { get; set; }
        public bool IsConnected
        {
            get { return Sdk.IsConnected(); }
        }

        private readonly int ticksPerSecond;
        private readonly List<Module> modules;
        private Thread thread;
        private System.Timers.Timer windowUpdater;

        public API(int ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            this.thread = new Thread(StartThread);

            windowUpdater = new System.Timers.Timer(500);
            windowUpdater.Elapsed += UpdateWindows;
            windowUpdater.Start();

            modules = new List<Module>();
            Sdk = new iRacingSDK();

            Sdk.Startup();
        }

        private void UpdateWindows(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TMTVO.Controller.TMTVO.Instance.Controls.UpdateLaunchButton(this);
            }));
        }

        private void RunApi()
        {
            long maxDelay = 1000L / ticksPerSecond;

            while (Run)
            {
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
                    Sdk.Startup();
                    continue;
                }

                Application.Current.Dispatcher.Invoke(new Action(TMTVO.Controller.TMTVO.Instance.Controls.UpdateWindow));

                long end = Environment.TickCount;
                long time = end - start;
                int sleepTime = (int)(maxDelay - time);

                if (time > 0)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        TMTVO.Controller.TMTVO.Instance.Controls.FpsItem.Content = (1000F / ((float)time)).ToString("0.0") + " FPS";
                        TMTVO.Controller.TMTVO.Instance.Controls.MsItem.Content = time + " ms";
                    }));

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

            TMTVO.Controller.TMTVO.Instance.Controls.Reset();
        }

        public void Start()
        {
            try
            {
                thread.Start();
            }
            catch (ThreadStateException)
            {
#pragma warning disable
                thread.Resume();
            }
        }

        private void StartThread(object obj)
        {
            RunApi();
        }

        public void Stop()
        {
#pragma warning disable
            thread.Suspend();
            ResetModules();
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
    }
}
