using iRSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Api
{
    public interface IAPI
    {
        bool Run { get; set; }
        iRacingSDK Sdk { get; }
        bool IsConnected { get; }
        void AddModule(Module module);
        void RemoveModule(Module module);
        void UpdateModules();
        void UpdateModules(string lines);
        void Start();
        void Stop();
        object GetData(string key);
        void HideUI();
        void SwitchCamera(int driver, int camera);
        void ReplaySetPlaySpeed(int playspeed, int slowmotion);
        void ReplaySetPlayPosition(ReplayPositionModeTypes mode, int position);
        void ReplaySearch(ReplaySearchModeTypes mode, int position);
        void Pause();
        void Play();
    }
}
