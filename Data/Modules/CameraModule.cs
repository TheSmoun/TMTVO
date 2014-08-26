using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMTVO.Api;
using Yaml;

namespace TMTVO.Data.Modules
{
    public class CameraModule : Module
    {
        public List<Camera> Cameras { get; private set; }
        public int CurrentCamera { get; set; }
        public int WantedCamera { get; set; }

        private iRacingControls window;

        public CameraModule(iRacingControls window) : base("CameraModule")
        {
            Cameras = new List<Camera>();
            this.window = window;
            this.window.CameraModule = this;

            CurrentCamera = 0;
            WantedCamera = 0;
        }

        public Camera FindCamera(int id)
        {
            Camera cam = Cameras.Find(c => c.Id == id);
            if (cam != null)
                return cam;

            return new Camera();
        }

        public override void Update(ConfigurationSection rootNode, API api)
        {
            int newIndex = (int)api.GetData("CamGroupNumber");
            if (newIndex != CurrentCamera)
            {
                CurrentCamera = newIndex;
                window.UpdateSelectedCamera(CurrentCamera);
            }

            List<Dictionary<string, object>> groups = rootNode.GetMapList("CameraInfo.Groups");
            if (groups.Count == Cameras.Count)
                return;

            bool added = false;
            foreach (Dictionary<string, object> dict in groups)
            {
                int id = int.Parse(dict.GetDictValue("GroupNum"));
                if (Cameras.FindIndex(c => c.Id == id) >= 0)
                    continue;

                Camera cam = new Camera();
                cam.Id = id;
                cam.Name = dict.GetDictValue("GroupName");
                Cameras.Add(cam);
                added = true;
            }

            if (added)
                Application.Current.Dispatcher.Invoke(new Action(window.UpdateCameras));
        }

        public override void Reset()
        {
            Cameras.Clear();
            CurrentCamera = 0;
            WantedCamera = 0;
        }
    }
}
